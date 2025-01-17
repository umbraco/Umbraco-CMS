using NUnit.Framework;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Events;

public class MoveEventInfoTests
{
    [TestCase("", "path", false)]
    [TestCase("entity", "", false)]
    [TestCase("entity", "path", true)]
    public void Can_Equate_Move_To_Recyclebin_Move_Event_Infos(string entity, string originalPath, bool expectedResult)
    {
        var recycleBinMoveEvent = new MoveToRecycleBinEventInfo<string>(entity, originalPath);
        var recycleBinMoveEventTwo = new MoveToRecycleBinEventInfo<string>("entity", "path");

        Assert.AreEqual(expectedResult, recycleBinMoveEvent.Equals(recycleBinMoveEventTwo));
    }

    [Test]
    public void Can_Equate_Move_To_Recyclebin_Move_Event_Infos_All_Params_Null_Or_Empty()
    {
        var recycleBinMoveEvent = new MoveToRecycleBinEventInfo<string>(string.Empty, string.Empty);
        var recycleBinMoveEventTwo = new MoveToRecycleBinEventInfo<string>(string.Empty, string.Empty);

        Assert.IsTrue(recycleBinMoveEvent.Equals(recycleBinMoveEventTwo));
    }

    [Test]
    public void Can_Equate_Move_Event_Infos_Parent_Key_Null()
    {
        var moveEvent = new MoveEventInfo<string>("entity", "path", 123, null);
        var moveEventTwo = new MoveEventInfo<string>("entity", "path", 123, null);
        Assert.IsTrue(moveEvent.Equals(moveEventTwo));
    }

    [Test]
    public void Can_Equate_Move_Event_Infos_All_Params_Null_Or_Empty()
    {
        var moveEvent = new MoveEventInfo<string>(string.Empty, string.Empty, 0, null);
        var moveEventTwo = new MoveEventInfo<string>(string.Empty, string.Empty, 0, null);
        Assert.IsTrue(moveEvent.Equals(moveEventTwo));
    }

    [TestCase(123, "entity", "", "063897F1-194A-4C42-B406-CA80DBC12968", false)]
    [TestCase(123, "", "path", "063897F1-194A-4C42-B406-CA80DBC12968", false)]
    [TestCase(12, "entity", "path", "063897F1-194A-4C42-B406-CA80DBC12968", false)]
    [TestCase(123, "entity", "path", "C6D6EA3E-C2B0-483F-B772-2F4D8BBF5027", false)]
    [TestCase(123, "entity", "path", "063897F1-194A-4C42-B406-CA80DBC12968", true)]
    public void Can_Equate_Move_Event_Infos(int parentId, string entity, string originalPath, Guid parentKey, bool expectedResult)
    {
        var moveEvent = new MoveEventInfo<string>(entity, originalPath, parentId, parentKey);
        var moveEventTwo = new MoveEventInfo<string>("entity", "path", 123, new Guid("063897F1-194A-4C42-B406-CA80DBC12968"));
        Assert.AreEqual(expectedResult, moveEvent.Equals(moveEventTwo));
    }
}
