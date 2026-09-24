using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BingWallPaper;

public class App
{
    private const string BingBaseUrl = "https://cn.bing.com";
    private static readonly Regex HomePageModelRegex = new(@"var\s+_model\s*=\s*(\{.*?\});", RegexOptions.Singleline);
    private static readonly Regex ImageSizeRegex = new("id=(.*?)(\\d+x\\d+)(.*?).webp");
    private readonly IHttpClientFactory _clientFactory;
    private readonly AppOption _appOption;

    public App(IOptionsMonitor<AppOption> appOption, IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _appOption = appOption.CurrentValue;
    }

    public async Task RunAsync(string[] args)
    {
        var data = await GetWallPaperUrl();
        var now = DateTime.Now;

        var filename = Path.Combine(
            Directory.GetCurrentDirectory(),
            _appOption.SavePath,
            now.ToString("yyyy"),
            now.ToString("MM"),
            now.ToString("dd") + ".json"
        );

        var dir = Path.GetDirectoryName(filename);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        Console.WriteLine(filename);


        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
        }));
    }


    private async Task<BingWallPaperInfo> GetWallPaperUrl()
    {
        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");
        var response = await client.GetAsync(BingBaseUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var wallPaperInfo = TryGetWallPaperFromHomePage(content);
        if (wallPaperInfo is not null)
        {
            return wallPaperInfo;
        }

        var archiveResponse = await client.GetAsync($"{BingBaseUrl}/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN");
        archiveResponse.EnsureSuccessStatusCode();
        var archiveContent = await archiveResponse.Content.ReadAsStringAsync();

        return GetWallPaperInfo(content, archiveContent);
    }

    internal static BingWallPaperInfo GetWallPaperInfo(string homePageContent, string archiveContent)
        => TryGetWallPaperFromHomePage(homePageContent) ?? GetWallPaperFromArchive(archiveContent);

    internal static BingWallPaperInfo? TryGetWallPaperFromHomePage(string content)
    {
        var match = HomePageModelRegex.Match(content);
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups[1].Value))
        {
            return null;
        }

        try
        {
            var json = JObject.Parse(match.Groups[1].Value);
            var imageContent = json["MediaContents"]?[0]?["ImageContent"];
            if (imageContent is null)
            {
                return null;
            }

            var headline = imageContent["Headline"]?.ToString();
            var title = imageContent["Title"]?.ToString();
            var description = imageContent["Description"]?.ToString();
            var imageUrl = BuildAbsoluteUrl(imageContent["Image"]?["Url"]?.ToString());
            var ultraHighDef = BuildUltraHighDefUrl(imageUrl);
            var imageWallpaper = BuildAbsoluteUrl(imageContent["Image"]?["Wallpaper"]?.ToString());
            var mainText = imageContent["QuickFact"]?["MainText"]?.ToString();
            var copyright = imageContent["Copyright"]?.ToString();

            return new BingWallPaperInfo(headline,
                title,
                description,
                ultraHighDef,
                imageUrl,
                imageWallpaper,
                mainText,
                copyright);
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }

    internal static BingWallPaperInfo GetWallPaperFromArchive(string archiveContent)
    {
        var json = JObject.Parse(archiveContent);
        var image = json["images"]?.FirstOrDefault()
            ?? throw new InvalidOperationException("Bing image archive response did not include any images.");
        var imageBaseUrl = image["urlbase"]?.ToString()
            ?? throw new InvalidOperationException("Bing image archive response did not include urlbase.");
        var imageId = GetArchiveImageId(imageBaseUrl);
        var headline = NullIfWhiteSpace(image["headline"]?.ToString());
        var title = NullIfWhiteSpace(image["title"]?.ToString()) ?? NullIfWhiteSpace(image["caption"]?.ToString());
        var description = image["desc"]?.ToString();
        var imageUrl = BuildAbsoluteUrl(image["url"]?.ToString()) ?? BuildArchiveImageUrl(imageId, "1920x1080.webp");
        var ultraHighDef = BuildArchiveImageUrl(imageId, "UHD.jpg");
        var imageWallpaper = image["wallpaper"]?.ToString() is { Length: > 0 } wallpaper
            ? BuildAbsoluteUrl(wallpaper)
            : BuildArchiveImageUrl(imageId, "1920x1200.jpg", "rf=LaDigue_1920x1200.jpg");
        var copyright = image["copyright"]?.ToString();

        return new BingWallPaperInfo(headline,
            title,
            description,
            ultraHighDef,
            imageUrl,
            imageWallpaper,
            null,
            copyright);
    }

    private static string? BuildAbsoluteUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? value
            : $"{BingBaseUrl}{value}";
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string GetArchiveImageId(string imageBaseUrl)
    {
        const string marker = "th?id=";
        var index = imageBaseUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index >= 0
            ? imageBaseUrl[(index + marker.Length)..]
            : imageBaseUrl.TrimStart('/');
    }

    private static string BuildArchiveImageUrl(string imageId, string suffix, string? query = null)
    {
        var imageUrl = $"{BingBaseUrl}/th?id={imageId}_{suffix}";
        return string.IsNullOrWhiteSpace(query)
            ? imageUrl
            : $"{imageUrl}&{query}";
    }

    private static string? BuildUltraHighDefUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return imageUrl;
        }

        var match = ImageSizeRegex.Match(imageUrl);
        return match.Success
            ? imageUrl.Replace(match.Groups[2].Value, "UHD").Replace(".webp", ".jpg", StringComparison.OrdinalIgnoreCase)
            : imageUrl;
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="Headline"></param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="UltraHighDef">原图(最清晰）</param>
/// <param name="ImageUrl"></param>
/// <param name="ImageWallpaper"></param>
/// <param name="MainText"></param>
/// <param name="CopyRight"></param>
public record BingWallPaperInfo(
    string? Headline,
    string? Title,
    string? Description,
    string? UltraHighDef,
    string? ImageUrl,
    string? ImageWallpaper,
    string? MainText,
    string? CopyRight);

public class AppOption
{
    public string SavePath { get; init; } = string.Empty;
}