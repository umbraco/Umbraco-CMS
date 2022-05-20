namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

public interface IUmbracoEndpointBuilder
{
    /// <summary>
    ///     Final call during app building to configure endpoints
    /// </summary>
    /// <param name="configureUmbraco"></param>
    void WithEndpoints(Action<IUmbracoEndpointBuilderContext> configureUmbraco);
}
