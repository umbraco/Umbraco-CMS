using Umbraco.Cms.Web.Common.ApplicationModels;

namespace Umbraco.Cms.Web.Common.Attributes;


[AttributeUsage(AttributeTargets.Class)]
[Obsolete("No-op attribute. Scheduled for removal in Umbraco 18.")]
public sealed class UmbracoApiControllerAttribute : Attribute
{
}
