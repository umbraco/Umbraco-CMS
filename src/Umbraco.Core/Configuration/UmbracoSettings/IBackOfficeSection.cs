namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IBackOfficeSection
    {
        ITourSection Tours { get; }
        INodeEditsSection NodeEdits { get; }
    }
}
