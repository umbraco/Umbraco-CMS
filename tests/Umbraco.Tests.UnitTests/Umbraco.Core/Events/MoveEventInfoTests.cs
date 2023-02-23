using NUnit.Framework;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Events;

public class MoveEventInfoTests
{
    [Test]
    public void Can_Equate_Move_To_Recyclebin_Move_Event_Infos()
    {
        string testString = "Test";
        var recycleBinMoveEvent = new MoveToRecycleBinEventInfo<string>(testString, string.Empty);
        var recycleBinMoveEventTwo = new MoveToRecycleBinEventInfo<string>(testString, string.Empty);

        Assert.IsTrue(recycleBinMoveEvent.Equals(recycleBinMoveEventTwo));
    }

    [Test]
    [TestCase(0, "00000000-0000-0000-0000-000000000000")]
    [TestCase(8, "979344F9-E0A4-495E-8A68-88F05F64EBAB")]
    public void Can_Equate_Move_Event_Infos(int parentId, Guid parentKey)
    {
        string testString = "Test";
        var recycleBinMoveEvent = new MoveEventInfo<string>(testString, string.Empty, parentId, parentKey);
        var recycleBinMoveEventTwo = new MoveEventInfo<string>(testString, string.Empty, parentId, parentKey);

        Assert.IsTrue(recycleBinMoveEvent.Equals(recycleBinMoveEventTwo));
    }
}
