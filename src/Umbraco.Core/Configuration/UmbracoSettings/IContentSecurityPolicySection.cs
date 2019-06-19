namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentSecurityPolicySection : IUmbracoConfigurationSection
    {
        bool EnableNonce { get; }
        IContentSecurityPolicySrc DefaultSrc { get; }
        IContentSecurityPolicySrc FontSrc { get; }
        IContentSecurityPolicySrc ConnectSrc { get; }
        IContentSecurityPolicySrc FrameSrc { get; }
        IContentSecurityPolicySrc ImgSrc { get; }
        IContentSecurityPolicySrc ManifestSrc { get; }
        IContentSecurityPolicySrc MediaSrc { get; }
        IContentSecurityPolicySrc ObjectSrc { get; }
        IContentSecurityPolicySrc ScriptSrc { get; }
        IContentSecurityPolicySrc StyleSrc { get; }
        IContentSecurityPolicySrc PrefetchSrc { get; }
        IContentSecurityPolicySrc WorkerSrc { get; }
        string ReportUri { get; }
        string ReportTo { get; }
    }
}
