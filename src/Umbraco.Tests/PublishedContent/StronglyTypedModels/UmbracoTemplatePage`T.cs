using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    public abstract class UmbracoTemplatePage<T> : UmbracoTemplatePage where T : TypedModelBase
    {
        protected override void InitializePage()
        {
            base.InitializePage();

            //set the model to the current node if it is not set, this is generally not the case
			if (Model != null)
			{
			    //Map CurrentModel here
                var constructorInfo = typeof(T).GetConstructor(new []{typeof(IPublishedContent)});
			    if (constructorInfo != null)
			    {
			        TypedModel = constructorInfo.Invoke(new object[]{Model.Content}) as T;
			    }
			}
        }

        /// <summary>
        /// Returns the a strongly typed model
        /// </summary>
        public T TypedModel { get; private set; }
    }
}