using Umbraco.Web.Mvc;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    public abstract class UmbracoTemplatePage<T> : UmbracoTemplatePage where T : TypedModelBase, new()
    {
        protected override void InitializePage()
        {
            base.InitializePage();

            //set the model to the current node if it is not set, this is generally not the case
			if (Model != null)
			{
			    //Map CurrentModel here
                TypedModel = new T();
                TypedModel.Add(Model.Content);
			}
        }

        /// <summary>
        /// Returns the a strongly typed model
        /// </summary>
        public T TypedModel { get; private set; }
    }
}