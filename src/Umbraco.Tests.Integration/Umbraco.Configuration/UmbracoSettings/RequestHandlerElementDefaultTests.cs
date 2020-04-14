using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
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
