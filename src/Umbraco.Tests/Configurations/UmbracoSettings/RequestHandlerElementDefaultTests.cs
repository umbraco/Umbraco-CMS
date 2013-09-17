using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class RequestHandlerElementDefaultTests : RequestHandlerElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}