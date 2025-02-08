using PlexVideoConverter.Models;
using PlexVideoConverter.Models.FileBrowser;
using PlexVideoConverter.Services;
using PlexVideoConverter.Services.FileBrowser;

namespace PvcTest;

public class FileBrowserTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestGetRootDrives()
    {
        var drives = FileBrowserService.GetRootDrives();
        Assert.NotNull(drives);
    }

    [Test]
    public void TestGetChildren()
    {
        var childRequest = new FileChildrenRequest();
        childRequest.Path = "C:/";
        childRequest.IncludeDirectories = true;
        childRequest.IncludeFiles = true;
        var childNodes = FileBrowserService.GetDirectoryChildrenNodes(childRequest);
        Assert.NotNull(childNodes);
    }
}