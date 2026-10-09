using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Membership;

[TestFixture]
public class IdentityCreationResultTests
{
    [Test]
    public void Fail_Creates_Unsuccessful_Result_With_Error_Message()
    {
        IdentityCreationResult result = IdentityCreationResult.Fail("something went wrong");

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeded, Is.False);
            Assert.That(result.CancelledByNotification, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("something went wrong"));
        });
    }

    [Test]
    public void Cancel_Creates_Unsuccessful_Result_Flagged_As_Cancelled_By_Notification()
    {
        IdentityCreationResult result = IdentityCreationResult.Cancel();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeded, Is.False);
            Assert.That(result.CancelledByNotification, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
        });
    }

    [Test]
    public void New_Result_Is_Neither_Succeeded_Nor_Cancelled_By_Default()
    {
        var result = new IdentityCreationResult();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeded, Is.False);
            Assert.That(result.CancelledByNotification, Is.False);
            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.InitialPassword, Is.Null);
        });
    }
}
