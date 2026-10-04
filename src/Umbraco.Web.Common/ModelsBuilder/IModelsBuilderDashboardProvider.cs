namespace Umbraco.Cms.Web.Common.ModelsBuilder;

[Obsolete("No longer used, as the Models Builder dashboard gets its data from the Management API. Scheduled for removal in Umbraco 19.")]
public interface IModelsBuilderDashboardProvider
{
    string? GetUrl();
}
