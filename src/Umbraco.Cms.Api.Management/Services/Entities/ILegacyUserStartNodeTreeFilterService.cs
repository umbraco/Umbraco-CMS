namespace Umbraco.Cms.Api.Management.Services.Entities;

[Obsolete("This is only needed for backwards compatibility. Scheduled for removal in Umbraco 19.")]
internal interface ILegacyUserStartNodeTreeFilterService
{
    [Obsolete("This is only needed for backwards compatibility. Scheduled for removal in Umbraco 19.")]
    int[] GetUserStartNodeIds();

    [Obsolete("This is only needed for backwards compatibility. Scheduled for removal in Umbraco 19.")]
    string[] GetUserStartNodePaths();
}
