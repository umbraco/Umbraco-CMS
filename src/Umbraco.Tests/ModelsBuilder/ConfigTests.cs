using System.Configuration;
using NUnit.Framework;
using Umbraco.ModelsBuilder.Embedded.Configuration;

namespace Umbraco.Tests.ModelsBuilder
{
    [TestFixture]
    public class ModelsBuilderConfigTests
    {
        [Test]
        public void Test1()
        {
            var config = new ModelsBuilderConfig(modelsNamespace: "test1");
            Assert.AreEqual("test1", config.ModelsNamespace);
        }

        [Test]
        public void Test2()
        {
            var config = new ModelsBuilderConfig(modelsNamespace: "test2");
            Assert.AreEqual("test2", config.ModelsNamespace);
        }

        [Test]
        public void DefaultModelsNamespace()
        {
            var config = new ModelsBuilderConfig();
            Assert.AreEqual(ModelsBuilderConfig.DefaultModelsNamespace, config.ModelsNamespace);
        }

        [TestCase("c:/path/to/root", "~/dir/models", false, "c:\\path\\to\\root\\dir\\models")]
        [TestCase("c:/path/to/root", "~/../../dir/models", true, "c:\\path\\dir\\models")]
        [TestCase("c:/path/to/root", "c:/another/path/to/elsewhere", true, "c:\\another\\path\\to\\elsewhere")]
        public void GetModelsDirectoryTests(string root, string config, bool acceptUnsafe, string expected)
        {
            Assert.AreEqual(expected, ModelsBuilderConfig.GetModelsDirectory(root, config, acceptUnsafe));
        }

        [TestCase("c:/path/to/root", "~/../../dir/models", false)]
        [TestCase("c:/path/to/root", "c:/another/path/to/elsewhere", false)]
        public void GetModelsDirectoryThrowsTests(string root, string config, bool acceptUnsafe)
        {
            Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var modelsDirectory = ModelsBuilderConfig.GetModelsDirectory(root, config, acceptUnsafe);
            });
        }
    }
}
