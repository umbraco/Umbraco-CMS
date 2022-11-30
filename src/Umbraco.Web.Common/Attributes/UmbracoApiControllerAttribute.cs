using Umbraco.Cms.Web.Common.ApplicationModels;

namespace Umbraco.Cms.Web.Common.Attributes;

/// <summary>
///     When present on a controller then <see cref="UmbracoApiBehaviorApplicationModelProvider" /> conventions will apply
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UmbracoApiControllerAttribute : Attribute
{
}
