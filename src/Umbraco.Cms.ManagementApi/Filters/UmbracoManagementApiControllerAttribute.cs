using Umbraco.Cms.ManagementApi.Filters;

namespace Umbraco.Cms.Web.Common.Attributes;

/// <summary>
///     When present on a controller then <see cref="UmbracoManagementApiBehaviorApplicationModelProvider" /> conventions will apply
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UmbracoManagementApiControllerAttribute : Attribute
{
}
