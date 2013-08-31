using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ContentScriptEditorElementTests : UmbracoSettingsTests
    {
        [Test]
        public void ScriptFolderPath()
        {
            Assert.IsTrue(Section.Content.ScriptEditor.ScriptFolderPath.Value == "/scripts");
        }
        [Test]
        public void ScriptFileTypes()
        {
            Assert.IsTrue(Section.Content.ScriptEditor.ScriptFileTypes.All(x => "js,xml".Split(',').Contains(x)));
        }
        [Test]
        public void DisableScriptEditor()
        {
            Assert.IsTrue(Section.Content.ScriptEditor.DisableScriptEditor.Value == false);
        }
    }
}