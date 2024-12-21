﻿using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BingWallPaper;

public class App
{
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
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        Console.WriteLine(filename);
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
        var url = "https://cn.bing.com";
        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var reg = new Regex("var _model =(\\{.*?\\});", RegexOptions.Singleline);
        var match = reg.Match(content);
        var json = JObject.Parse(match.Groups[1].Value);

        var imageContent = json["MediaContents"]?[0]?["ImageContent"]!;

        var headline = imageContent["Headline"]?.ToString();
        var title = imageContent["Title"]?.ToString();
        var description = imageContent["Description"]?.ToString();
        var imageUrl = imageContent["Image"]?["Url"]?.ToString();
        var imageWallpaper = $"{url}{imageContent["Image"]?["Wallpaper"]}";
        var mainText = imageContent["QuickFact"]?["MainText"]?.ToString();
        var copyright = imageContent["Copyright"]?.ToString();
        var quickFactMainText = imageContent["QuickFact"]?["MainText"]?.ToString();

        return new BingWallPaperInfo(headline, 
            title, 
            description, 
            imageUrl, 
            imageWallpaper, 
            mainText, 
            copyright,
            quickFactMainText);
    }
}

public record BingWallPaperInfo(
    string? Headline,
    string? Title,
    string? Description,
    string? ImageUrl,
    string ImageWallpaper,
    string? MainText,
    string? CopyRight,
    string? QuickFactMainText);

public class AppOption
{
    public string SavePath { get; init; }
}