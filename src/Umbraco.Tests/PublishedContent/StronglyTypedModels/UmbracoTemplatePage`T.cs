using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    /// <summary>
    /// Represents a basic extension of the UmbracoTemplatePage, which allows you to specify
    /// the type of a strongly typed model that inherits from TypedModelBase.
    /// The model is exposed as TypedModel.
    /// </summary>
    /// <typeparam name="T">Type of the model to create/expose</typeparam>
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