namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents Umbraco features that can be toggled
    /// </summary>
    public class UmbracoFeatures
    {
        public UmbracoFeatures()
        {
            DisabledFeatures = new DisabledFeatures();
        }

        public DisabledFeatures DisabledFeatures { get; set; }

        //NOTE: Currently we can only Disable features but maybe some day we could enable non standard features too
    }
}