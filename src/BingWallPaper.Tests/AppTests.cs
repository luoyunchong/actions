using Xunit;

namespace BingWallPaper.Tests;

public class AppTests
{
    [Fact]
    public void GetWallPaperInfo_FallsBackToArchiveWhenHomePageParsingFails()
    {
        const string homePageContent = "<html></html>";
        const string archiveContent = """
            {
              "images": [
                {
                  "url": "/th?id=OHR.Archive_1920x1080.webp&rf=LaDigue_1920x1080.webp&pid=hp",
                  "urlbase": "/th?id=OHR.Archive",
                  "title": "",
                  "caption": "归档标题",
                  "desc": "归档描述",
                  "copyright": "归档版权"
                }
              ]
            }
            """;

        var info = App.GetWallPaperInfo(homePageContent, archiveContent);

        Assert.Null(info.Headline);
        Assert.Equal("归档标题", info.Title);
        Assert.Equal("归档描述", info.Description);
        Assert.Equal("https://cn.bing.com/th?id=OHR.Archive_UHD.jpg", info.UltraHighDef);
    }

    [Fact]
    public void GetWallPaperFromArchive_ThrowsWhenImagesAreMissing()
    {
        const string archiveContent = """{"images": []}""";

        var exception = Assert.Throws<InvalidOperationException>(() => App.GetWallPaperFromArchive(archiveContent));

        Assert.Contains("did not include any images", exception.Message);
    }

    [Fact]
    public void GetWallPaperFromArchive_BuildsArchiveUrlsFromUrlBase()
    {
        const string archiveContent = """
            {
              "images": [
                {
                  "urlbase": "/th?id=OHR.Sample",
                  "title": "示例标题",
                  "desc": "示例描述",
                  "copyright": "示例版权"
                }
              ]
            }
            """;

        var info = App.GetWallPaperFromArchive(archiveContent);

        Assert.Equal("https://cn.bing.com/th?id=OHR.Sample_1920x1080.webp", info.ImageUrl);
        Assert.Equal("https://cn.bing.com/th?id=OHR.Sample_1920x1200.jpg&rf=LaDigue_1920x1200.jpg", info.ImageWallpaper);
        Assert.Equal("https://cn.bing.com/th?id=OHR.Sample_UHD.jpg", info.UltraHighDef);
    }
}
