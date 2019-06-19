namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentSecurityPolicySrc : IUmbracoConfigurationSection
    {
        bool UnsafeInline { get; }
        bool UnsafeEval { get; }
        bool None { get; }
        bool Self { get; }
        string Hosts { get; }
    }
}
