using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;

namespace HackerNews;

public class App
{
    private readonly ILogger<App> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly AppOption _appOption;
    public App(ILogger<App> logger, IOptionsMonitor<AppOption> appOption, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _appOption = appOption.CurrentValue;
    }

    public async Task RunAsync(string[] args)
    {
        string content = await GetHeadlines(DateTime.UtcNow);
        _logger.LogInformation(content);

        _logger.LogInformation(JsonConvert.SerializeObject(_appOption));

        var jwtToken = GetAccessToken();

        var appClient = new GitHubClient(new ProductHeaderValue(_appOption.Repo))
        {
            Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
        };

        var response = await appClient.GitHubApps.CreateInstallationToken(_appOption.InstallationId);

        var installationClient = new GitHubClient(new ProductHeaderValue(_appOption.Repo))
        {
            Credentials = new Credentials(response.Token)
        };

        string title = $"Hacker News Top 10 {DateTime.Now:yyyy-MM-dd}";
        switch (_appOption.HacknewsType)
        {
            case HacknewsType.Daily:
                title = $"Hacker News Daily Top 10 {DateTime.Now:yyyy-MM-dd}";
                break;
            case HacknewsType.Weekly:
                title = $"Hacker News Weekly Top 10 {DateTime.Now:yyyy-MM-dd}";
                break;
            case HacknewsType.Monthly:
                title = $"Hacker News Monthly Top 10 {DateTime.Now:yyyy-MM-dd}";
                break;
        }

        var createIssue = new NewIssue(title)
        {
            Body = content
        };

        var issue = await installationClient.Issue.Create(_appOption.Owner, _appOption.Repo, createIssue);
        _logger.LogInformation($"Issue created: {issue.HtmlUrl}");

        await installationClient.Issue.LockUnlock.Lock(_appOption.Owner, _appOption.Repo, issue.Number);
        _logger.LogInformation($"Issue locked: {issue.HtmlUrl}");

    }
    public string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
    private string GetAccessToken()
    {
        var generator = new GitHubJwt.GitHubJwtFactory(
            new GitHubJwt.StringPrivateKeySource(Base64Decode(_appOption.AppPrivateKey)),
            new GitHubJwt.GitHubJwtFactoryOptions
            {
                AppIntegrationId = _appOption.AppIntegrationId,
                ExpirationSeconds = 600
            }
        );
        var jwtToken = generator.CreateEncodedJwtToken();
        return jwtToken;
    }


    public async Task<string> GetHeadlines(DateTime date)
    {
        var client = _clientFactory.CreateClient();

        long endTime = new DateTimeOffset(date).ToUnixTimeSeconds();
        long startTime = endTime - (25 * 60 * 60);

        switch (_appOption.HacknewsType)
        {
            case HacknewsType.Daily:
                startTime = endTime - (25 * 60 * 60);
                break;
            case HacknewsType.Weekly:
                startTime = endTime - (7 * 24 * 60 * 60 + 12 * 60 * 60);
                break;
            case HacknewsType.Monthly:
                startTime = endTime - (31 * 24 * 60 * 60 + 12 * 60 * 60);
                break;
            default: break;
        }

        string url = $"https://hn.algolia.com/api/v1/search?numericFilters=created_at_i>{startTime},created_at_i<{endTime}";
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseBody);
        var top10Objs = json["hits"].Take(10);

        var contents = top10Objs.Select((obj, i) =>
        {
            string title = obj["title"]?.ToString();
            string createdAt = obj["created_at"]?.ToString();
            string url = obj["url"]?.ToString();
            string author = obj["author"]?.ToString();
            int points = obj["points"]?.ToObject<int>() ?? 0;
            string objectID = obj["objectID"]?.ToString();
            int numComments = obj["num_comments"]?.ToObject<int>() ?? 0;

            if (string.IsNullOrEmpty(url))
            {
                url = $"https://news.ycombinator.com/item?id={objectID}";
            }

            return $"{i + 1}. **[{title}]({url})**\n{points} points by [{author}](https://news.ycombinator.com/user?id={author}) {createdAt} | [{numComments} comments](https://news.ycombinator.com/item?id={objectID})\n";
        }).Aggregate((current, next) => current + next);

        return $"{contents}\n<p align=\"right\"><a href=\"https://github.com/{_appOption.Owner}/{_appOption.Repo}\"> <i>❤️ Sponsor the author</i></a> </p>\n";

    }
}