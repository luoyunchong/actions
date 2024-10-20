namespace HackerNews;

public class AppOption
{
    public string AppPrivateKey { get; set; }
    public int AppIntegrationId { get; set; }
    public int InstallationId { get; set; }
    public string Owner { get; set; }
    public string Repo { get; set; }

    public HacknewsType HacknewsType { get; set; }
}

public enum HacknewsType
{
    Daily = 10,
    Weekly = 20,
    Monthly = 30
}