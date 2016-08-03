using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using ImageProcessor.Web.Processors;
using ImageProcessor.Web.Configuration;
using ImageProcessor.Web.HttpModules;
using Umbraco.Core;

namespace Umbraco.Web.UI
{
    public class ImageProcessorValidation : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
        }

        private static void ImageProcessingModule_ValidatingRequest(object sender, ImageProcessor.Web.Helpers.ValidatingRequestEventArgs e)
        {
// Blur is disabled by default, add it to the list of available processors again
var configuration = ImageProcessorConfiguration.Instance;
var settings = new Dictionary<string, string>
{
    { "MaxSize", "15" },
    { "MaxSigma", "1.5" },
    { "MaxThreshold", "10" }
};

configuration.AvailableWebGraphicsProcessors.TryAdd(typeof(GaussianBlur), settings);

            // Nothing to process, return immediately
            if (string.IsNullOrWhiteSpace(e.QueryString))
                return;

            // Don't support alpha whatsoever
            var queryCollection = HttpUtility.ParseQueryString(e.QueryString);
            if (queryCollection.AllKeys.Contains("alpha"))
            {
                e.Cancel = true;
                return;
            }

            // If there's a crop parameter, force it to always just be a specific value
            if (queryCollection.AllKeys.Contains("crop"))
            {
                queryCollection["crop"] = "100,100,100,100";
                // this performs the reverse of ParseQueryString since the result of ParseQueryString
                // is actually an instance of System.Web.HttpValueCollection
                e.QueryString = queryCollection.ToString();
            }
        }
    }
}