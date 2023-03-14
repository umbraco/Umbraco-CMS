using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

// purpose: test the values returned by PublishedContentCache.GetRouteById
// and .GetByRoute (no caching at all, just routing nice URLs) including all
// the quirks due to hideTopLevelFromPath and backward compatibility.
public class UrlRoutesTests : PublishedSnapshotServiceTestBase
{
    private static string GetXmlContent(int templateId)
        => @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Doc ANY>
<!ATTLIST Doc id ID #REQUIRED>
]>
<root id=""-1"">
    <Doc id=""1000"" parentID=""-1"" level=""1"" path=""-1,1000"" nodeName=""A"" urlName=""a"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
        <Doc id=""1001"" parentID=""1000"" level=""2"" path=""-1,1000,1001"" nodeName=""B"" urlName=""b"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            <Doc id=""1002"" parentID=""1001"" level=""3"" path=""-1,1000,1001,1002"" nodeName=""C"" urlName=""c"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" writerName=""admin"" creatorName=""admin"" isDoc="""">
                <Doc id=""1003"" parentID=""1002"" level=""4"" path=""-1,1000,1001,1002,1003"" nodeName=""D"" urlName=""d"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" writerName=""admin"" creatorName=""admin"" isDoc="""">
                </Doc>
            </Doc>
        </Doc>
    </Doc>
    <Doc id=""2000"" parentID=""-1"" level=""1"" path=""-1,2000"" nodeName=""X"" urlName=""x"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
        <Doc id=""2001"" parentID=""2000"" level=""2"" path=""-1,2000,2001"" nodeName=""Y"" urlName=""y"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            <Doc id=""2002"" parentID=""2001"" level=""3"" path=""-1,2000,2001,2002"" nodeName=""Z"" urlName=""z"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            </Doc>
        </Doc>
        <Doc id=""2003"" parentID=""2000"" level=""2"" path=""-1,2000,2003"" nodeName=""A"" urlName=""a"" sortOrder=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
        </Doc>
        <Doc id=""2004"" parentID=""2000"" level=""2"" path=""-1,2000,2004"" nodeName=""B"" urlName=""b"" sortOrder=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            <Doc id=""2005"" parentID=""2004"" level=""3"" path=""-1,2000,2004,2005"" nodeName=""C"" urlName=""c"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            </Doc>
            <Doc id=""2006"" parentID=""2004"" level=""3"" path=""-1,2000,2004,2006"" nodeName=""E"" urlName=""e"" sortOrder=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" +
           templateId +
           @""" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" writerName=""admin"" creatorName=""admin"" isDoc="""">
            </Doc>
        </Doc>
    </Doc>
</root>";

    /*
     * Just so it's documented somewhere, as of jan. 2017, routes obey the following pseudo-code:

GetByRoute(route, hide = null):

route is "[id]/[path]"

hide = hide ?? global.hide

root = id ? node(id) : document

content = cached(route) ?? DetermineIdByRoute(route, hide)

# route is "1234/path/to/content", finds "content"
# but if there is domain 5678 on "to", the *true* route of "content" is "5678/content"
# so although the route does match, we don't cache it
# there are not other reason not to cache it

if content and no domain between root and content:
    cache route (as trusted)

return content


DetermineIdByRoute(route, hide):

route is "[id]/[path]"

try return NavigateRoute(id ?? 0, path, hide:hide)
return null


NavigateRoute(id, path, hide):

if path:
    if id:
        start = node(id)
    else:
        start = document

    # 'navigate ... from ...' uses lowest sortOrder in case of collision

    if hide and ![id]:
        # if hiding, then for "/foo" we want to look for "/[any]/foo"
        for each child of start:
            try return navigate path from child

        # but if it fails, we also want to try "/foo"
        # fail now if more than one part eg "/foo/bar"
        if path is "/[any]/...":
            fail

    try return navigate path from start

else:
    if id:
        return node(id)
    else:
        return root node with lowest sortOrder


GetRouteById(id):


route = cached(id)
if route:
    return route

# never cache the route, it may be colliding

route = DetermineRouteById(id)
if route:
    cache route (as not trusted)

return route



DetermineRouteById(id):


node = node(id)

walk up from node to domain or root, assemble parts = URL segments

if !domain and global.hide:
    if id.parent:
        # got /top/[path]content, can remove /top
        remove top part
    else:
        # got /content, should remove only if it is the
        # node with lowest sort order
        root = root node with lowest sortOrder
        if root == node:
            remove top part

compose path from parts
route = assemble "[domain.id]/[path]"
return route

     */

    /*
     * The Xml structure for the following tests is:
     *
     * root
     *   A 1000
     *     B 1001
     *       C 1002
     *         D 1003
     *   X 2000
     *     Y 2001
     *       Z 2002
     *     A 2003
     *     B 2004
     *       C 2005
     *       E 2006
     *
     */

    [TestCase(1000, false, "/a")]
    [TestCase(1001, false, "/a/b")]
    [TestCase(1002, false, "/a/b/c")]
    [TestCase(1003, false, "/a/b/c/d")]
    [TestCase(2000, false, "/x")]
    [TestCase(2001, false, "/x/y")]
    [TestCase(2002, false, "/x/y/z")]
    [TestCase(2003, false, "/x/a")]
    [TestCase(2004, false, "/x/b")]
    [TestCase(2005, false, "/x/b/c")]
    [TestCase(2006, false, "/x/b/e")]
    public void GetRouteByIdNoHide(int id, bool hide, string expected)
    {
        GlobalSettings.HideTopLevelNodeFromPath = hide;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        var route = cache.GetRouteById(false, id);
        Assert.AreEqual(expected, route);
    }

    [TestCase(1000, true, "/")]
    [TestCase(1001, true, "/b")]
    [TestCase(1002, true, "/b/c")]
    [TestCase(1003, true, "/b/c/d")]
    [TestCase(2000, true, "/x")]
    [TestCase(2001, true, "/y")]
    [TestCase(2002, true, "/y/z")]
    [TestCase(2003, true, "/a")]
    [TestCase(2004, true, "/b")] // collision!
    [TestCase(2005, true, "/b/c")] // collision!
    [TestCase(2006, true, "/b/e")] // risky!
    public void GetRouteByIdHide(int id, bool hide, string expected)
    {
        GlobalSettings.HideTopLevelNodeFromPath = hide;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        var route = cache.GetRouteById(false, id);
        Assert.AreEqual(expected, route);
    }

    [Test]
    public void GetRouteByIdCache()
    {
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        var route = cache.GetRouteById(false, 1000);
        Assert.AreEqual("/a", route);
    }

    [TestCase("/", false, 1000)]
    [TestCase("/a", false, 1000)] // yes!
    [TestCase("/a/b", false, 1001)]
    [TestCase("/a/b/c", false, 1002)]
    [TestCase("/a/b/c/d", false, 1003)]
    [TestCase("/x", false, 2000)]
    public void GetByRouteNoHide(string route, bool hide, int expected)
    {
        GlobalSettings.HideTopLevelNodeFromPath = hide;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        const bool preview = false; // make sure we don't cache - but HOW? should be some sort of switch?!
        var content = cache.GetByRoute(preview, route);
        if (expected < 0)
        {
            Assert.IsNull(content);
        }
        else
        {
            Assert.IsNotNull(content);
            Assert.AreEqual(expected, content.Id);
        }
    }

    [TestCase("/", true, 1000)]
    [TestCase("/a", true, 2003)]
    [TestCase("/a/b", true, -1)]
    [TestCase("/x", true, 2000)] // oops!
    [TestCase("/x/y", true, -1)] // yes!
    [TestCase("/y", true, 2001)]
    [TestCase("/y/z", true, 2002)]
    [TestCase("/b", true, 1001)] // (hence the 2004 collision)
    [TestCase("/b/c", true, 1002)] // (hence the 2005 collision)
    public void GetByRouteHide(string route, bool hide, int expected)
    {
        GlobalSettings.HideTopLevelNodeFromPath = hide;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        const bool preview = false; // make sure we don't cache - but HOW? should be some sort of switch?!
        var content = cache.GetByRoute(preview, route);
        if (expected < 0)
        {
            Assert.IsNull(content);
        }
        else
        {
            Assert.IsNotNull(content);
            Assert.AreEqual(expected, content.Id);
        }
    }

    [Test]
    public void GetByRouteCache()
    {
        GlobalSettings.HideTopLevelNodeFromPath = false;

        var xml = GetXmlContent(1234);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        InitializedCache(kits, contentTypes, dataTypes);

        var cache = GetPublishedSnapshot().Content;

        var content = cache.GetByRoute(false, "/a/b/c");
        Assert.IsNotNull(content);
        Assert.AreEqual(1002, content.Id);
    }
}
