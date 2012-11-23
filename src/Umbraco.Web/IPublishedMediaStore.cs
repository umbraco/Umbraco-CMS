using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web
{
	/// <summary>
	/// Defines the methods to access published media
	/// </summary>
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1153", "We need to create something like the IPublishListener interface to have proper published content storage. We'll also need to publicize the resolvers so people can set a resolver at app startup.")]
	internal interface IPublishedMediaStore : IPublishedStore
	{
	}
}