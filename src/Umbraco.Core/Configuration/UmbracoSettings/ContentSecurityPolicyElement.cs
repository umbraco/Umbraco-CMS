using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentSecurityPolicyElement : UmbracoConfigurationElement, IContentSecurityPolicySection
    {
        [ConfigurationProperty("enableNonce", DefaultValue = true)]
        internal bool EnableNonce => (bool)base["enableNonce"];

        [ConfigurationProperty("defaultSrc")]
        internal ContentSecurityPolicySrcElement DefaultSrc => (ContentSecurityPolicySrcElement)this["defaultSrc"];

        [ConfigurationProperty("fontSrc")]
        internal ContentSecurityPolicySrcElement FontSrc => (ContentSecurityPolicySrcElement)this["fontSrc"];

        [ConfigurationProperty("connectSrc")]
        internal ContentSecurityPolicySrcElement ConnectSrc => (ContentSecurityPolicySrcElement)this["connectSrc"];

        [ConfigurationProperty("frameSrc")]
        internal ContentSecurityPolicySrcElement FrameSrc => (ContentSecurityPolicySrcElement)this["frameSrc"];

        [ConfigurationProperty("imgSrc")]
        internal ContentSecurityPolicySrcElement ImgSrc => (ContentSecurityPolicySrcElement)this["imgSrc"];

        [ConfigurationProperty("manifestSrc")]
        internal ContentSecurityPolicySrcElement ManifestSrc => (ContentSecurityPolicySrcElement)this["manifestSrc"];

        [ConfigurationProperty("mediaSrc")]
        internal ContentSecurityPolicySrcElement MediaSrc => (ContentSecurityPolicySrcElement)this["mediaSrc"];

        [ConfigurationProperty("objectSrc")]
        internal ContentSecurityPolicySrcElement ObjectSrc => (ContentSecurityPolicySrcElement)this["objectSrc"];

        [ConfigurationProperty("scriptSrc")]
        internal ContentSecurityPolicySrcElement ScriptSrc => (ContentSecurityPolicySrcElement)this["scriptSrc"];

        [ConfigurationProperty("styleSrc")]
        internal ContentSecurityPolicySrcElement StyleSrc => (ContentSecurityPolicySrcElement)this["styleSrc"];

        [ConfigurationProperty("prefetchSrc")]
        internal ContentSecurityPolicySrcElement PrefetchSrc => (ContentSecurityPolicySrcElement)this["prefetchSrc"];

        [ConfigurationProperty("workerSrc")]
        internal ContentSecurityPolicySrcElement WorkerSrc => (ContentSecurityPolicySrcElement)this["workerSrc"];

        [ConfigurationProperty("reportUri")]
        internal InnerTextConfigurationElement<string> ReportUri => GetOptionalTextElement<string>("reportUri", null);

        [ConfigurationProperty("reportTo")]
        internal InnerTextConfigurationElement<string> ReportTo => GetOptionalTextElement<string>("reportTo", null);

        bool IContentSecurityPolicySection.EnableNonce => EnableNonce;

        IContentSecurityPolicySrc IContentSecurityPolicySection.DefaultSrc => DefaultSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.FontSrc => FontSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.ConnectSrc => ConnectSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.FrameSrc => FrameSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.ImgSrc => ImgSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.ManifestSrc => ManifestSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.MediaSrc => MediaSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.ObjectSrc => ObjectSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.ScriptSrc => ScriptSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.StyleSrc => StyleSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.PrefetchSrc => PrefetchSrc;

        IContentSecurityPolicySrc IContentSecurityPolicySection.WorkerSrc => WorkerSrc;

        string IContentSecurityPolicySection.ReportUri => ReportUri;

        string IContentSecurityPolicySection.ReportTo => ReportTo;
    }
}
