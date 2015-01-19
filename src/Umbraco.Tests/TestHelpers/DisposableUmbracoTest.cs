using System.Web;
using Moq;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers
{
    //NOTE: This is just a POC! Looking at the simplest way to expose some code so people can very easily test
    // their Umbraco controllers, etc....
    public class DisposableUmbracoTest : DisposableObject
    {
        public ApplicationContext ApplicationContext { get; set; }
        public UmbracoContext UmbracoContext { get; set; }

        public DisposableUmbracoTest(ApplicationContext applicationContext)
        {
            //init umb context
            var umbctx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                applicationContext,
                true);

            Init(applicationContext, umbctx);
        }

        public DisposableUmbracoTest(ApplicationContext applicationContext, UmbracoContext umbracoContext)
        {
            Init(applicationContext, umbracoContext);
        }

        private void Init(ApplicationContext applicationContext, UmbracoContext umbracoContext)
        {
            ApplicationContext = applicationContext;
            UmbracoContext = umbracoContext;

            ApplicationContext.Current = applicationContext;
            UmbracoContext.Current = umbracoContext;

            Resolution.Freeze();
        }

        protected override void DisposeResources()
        {
            ApplicationContext.Current = null;
            UmbracoContext.Current = null;
            Resolution.Reset();
        }
    }
}