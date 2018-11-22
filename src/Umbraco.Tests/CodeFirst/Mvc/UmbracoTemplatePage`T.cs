using Umbraco.Web.Mvc;

namespace Umbraco.Tests.CodeFirst.Mvc
{
    public abstract class UmbracoTemplatePage<T> : UmbracoTemplatePage where T : class 
    {
        protected override void InitializePage()
        {
            base.InitializePage();

            //set the model to the current node if it is not set, this is generally not the case
			if (Model != null)
			{
			    //Map CurrentContent here
			}
        }

        /// <summary>
        /// Returns the a strongly typed model
        /// </summary>
        public T CurrentContent { get; private set; }
    }
}