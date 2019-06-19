using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentSecurityPolicyTests : UmbracoSettingsTests
    {
        [Test]
        public void DefaultSrcSelfIsTrue()
        {
            Assert.IsTrue(SettingsSection.Security.ContentSecurityPolicy.DefaultSrc.Self == true);
        }

        [Test]
        public void NonceIsEnabled()
        {
            Assert.IsTrue(SettingsSection.Security.HideDisabledUsersInBackoffice == false);
        }

        [Test]
        public void ScriptUnsafeInlineIsDisabled()
        {
            Assert.IsTrue(SettingsSection.Security.ContentSecurityPolicy.ScriptSrc?.UnsafeInline == false);
        }
        [Test]
        public void ScriptUnsafeEvalIsDisabled()
        {
            Assert.IsTrue(SettingsSection.Security.ContentSecurityPolicy.ScriptSrc?.UnsafeEval == false);
        }
    }
}
