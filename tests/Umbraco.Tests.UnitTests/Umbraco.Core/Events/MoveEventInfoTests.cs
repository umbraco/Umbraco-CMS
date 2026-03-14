using NUnit.Framework;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Events;

    /// <summary>
    /// Contains unit tests for the <see cref="MoveEventInfo"/> class in the Umbraco.Core.Events namespace.
    /// These tests verify the behavior and functionality of MoveEventInfo.
    /// </summary>
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

    /// <summary>
    /// Tests that two MoveToRecycleBinEventInfo instances with all parameters null or empty are considered equal.
    /// </summary>
    [Test]
    public void Can_Equate_Move_To_Recyclebin_Move_Event_Infos_All_Params_Null_Or_Empty()
    {
        var recycleBinMoveEvent = new MoveToRecycleBinEventInfo<string>(string.Empty, string.Empty);
        var recycleBinMoveEventTwo = new MoveToRecycleBinEventInfo<string>(string.Empty, string.Empty);

        Assert.IsTrue(recycleBinMoveEvent.Equals(recycleBinMoveEventTwo));
    }

    /// <summary>
    /// Tests that two MoveEventInfo instances with null parent keys are considered equal.
    /// </summary>
    [Test]
    public void Can_Equate_Move_Event_Infos_Parent_Key_Null()
    {
        var moveEvent = new MoveEventInfo<string>("entity", "path", 123, null);
        var moveEventTwo = new MoveEventInfo<string>("entity", "path", 123, null);
        Assert.IsTrue(moveEvent.Equals(moveEventTwo));
    }

    /// <summary>
    /// Tests that two MoveEventInfo instances with all parameters null or empty are considered equal.
    /// </summary>
    [Test]
    public void Can_Equate_Move_Event_Infos_All_Params_Null_Or_Empty()
    {
        var moveEvent = new MoveEventInfo<string>(string.Empty, string.Empty, 0, null);
        var moveEventTwo = new MoveEventInfo<string>(string.Empty, string.Empty, 0, null);
        Assert.IsTrue(moveEvent.Equals(moveEventTwo));
    }

    /// <summary>
    /// Verifies that two <see cref="MoveEventInfo{T}"/> instances are considered equal or not equal based on their constructor parameters.
    /// </summary>
    /// <param name="parentId">The ID of the parent entity for the move event under test.</param>
    /// <param name="entity">The entity value for the move event under test.</param>
    /// <param name="originalPath">The original path of the entity for the move event under test.</param>
    /// <param name="parentKey">The unique identifier (GUID) of the parent entity for the move event under test.</param>
    /// <param name="expectedResult">True if the two <see cref="MoveEventInfo{T}"/> instances are expected to be equal; otherwise, false.</param>
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
