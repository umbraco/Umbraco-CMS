using Umbraco.Cms.Web.Common.ApplicationModels;

namespace Umbraco.Cms.Web.Common.Attributes;


[AttributeUsage(AttributeTargets.Class)]
[Obsolete("No-op attribute. Will be removed in Umbraco 15.")]
public sealed class UmbracoApiControllerAttribute : Attribute
{
}
