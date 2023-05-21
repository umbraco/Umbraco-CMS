namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

public interface ITypeFinderConfig
{
    IEnumerable<string> AssembliesAcceptingLoadExceptions { get; }
}
