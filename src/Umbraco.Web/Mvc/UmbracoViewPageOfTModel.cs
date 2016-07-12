using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The View that umbraco front-end views inherit from
    /// </summary>
    public abstract class UmbracoViewPage<TModel> : WebViewPage<TModel>
    {
        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get
            {
                //we should always try to return the context from the data tokens just in case its a custom context and not 
                //using the UmbracoContext.Current, we will fallback to the singleton if necessary.
                var umbCtx = ViewContext.GetUmbracoContext()
                    //lastly, we will use the singleton, the only reason this should ever happen is is someone is rendering a page that inherits from this
                    //class and are rendering it outside of the normal Umbraco routing process. Very unlikely.
                    ?? UmbracoContext.Current;
                
                return umbCtx;
            }
        }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns the current PublishedContentRequest
        /// </summary>
        internal PublishedContentRequest PublishedContentRequest
        {
            get
            {
                //we should always try to return the object from the data tokens just in case its a custom object and not 
                //using the UmbracoContext.Current.
                //we will fallback to the singleton if necessary.
                if (ViewContext.RouteData.DataTokens.ContainsKey(Core.Constants.Web.PublishedDocumentRequestDataToken))
                {
                    return (PublishedContentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject(Core.Constants.Web.PublishedDocumentRequestDataToken);
                }
                //next check if it is a child action and see if the parent has it set in data tokens
                if (ViewContext.IsChildAction)
                {
                    if (ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey(Core.Constants.Web.PublishedDocumentRequestDataToken))
                    {
                        return (PublishedContentRequest)ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject(Core.Constants.Web.PublishedDocumentRequestDataToken);
                    }
                }

                //lastly, we will use the singleton, the only reason this should ever happen is is someone is rendering a page that inherits from this
                //class and are rendering it outside of the normal Umbraco routing process. Very unlikely.
                return UmbracoContext.Current.PublishedContentRequest;
            }
        }

        private UmbracoHelper _helper;
        private MembershipHelper _membershipHelper;

        /// <summary>
        /// Gets an UmbracoHelper
        /// </summary>
        /// <remarks>
        /// This constructs the UmbracoHelper with the content model of the page routed to
        /// </remarks>
        public virtual UmbracoHelper Umbraco
        {
            get
            {
                if (_helper == null)
                {
                    var model = ViewData.Model;
                    var content = model as IPublishedContent;
                    if (content == null && model is IRenderModel)
                        content = ((IRenderModel) model).Content;
                    _helper = content == null
                        ? new UmbracoHelper(UmbracoContext)
                        : new UmbracoHelper(UmbracoContext, content);
                }
                return _helper;
            }
        }

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return _membershipHelper ?? (_membershipHelper = new MembershipHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Ensure that the current view context is added to the route data tokens so we can extract it if we like
        /// </summary>
        /// <remarks>
        /// Currently this is required by mvc macro engines
        /// </remarks>
        protected override void InitializePage()
        {
            base.InitializePage();
            if (ViewContext.IsChildAction == false)
            {
                //this is used purely for partial view macros that contain forms 
                // and mostly just when rendered within the RTE - This should already be set with the 
                // EnsurePartialViewMacroViewContextFilterAttribute
                if (ViewContext.RouteData.DataTokens.ContainsKey(Constants.DataTokenCurrentViewContext) == false)
                {
                    ViewContext.RouteData.DataTokens.Add(Constants.DataTokenCurrentViewContext, ViewContext);
                }
            }

        }

        // maps model
        protected override void SetViewData(ViewDataDictionary viewData)
        {
            // capture the model before we tinker with the viewData
            var viewDataModel = viewData.Model;

            // map the view data (may change its type, may set model to null)
            viewData = MapViewDataDictionary(viewData, typeof (TModel));

            var culture = CultureInfo.CurrentCulture;
            // bind the model (use context culture as default, if available)
            if (UmbracoContext.PublishedContentRequest != null && UmbracoContext.PublishedContentRequest.Culture != null)
                culture = UmbracoContext.PublishedContentRequest.Culture;
            viewData.Model = RenderModelBinder.BindModel(viewDataModel, typeof (TModel), culture);

            // set the view data
            base.SetViewData(viewData);
        }

        // viewData is the ViewDataDictionary (maybe <TModel>) that we have
        // modelType is the type of the model that we need to bind to
        //
        // figure out whether viewData can accept modelType else replace it
        //
        private static ViewDataDictionary MapViewDataDictionary(ViewDataDictionary viewData, Type modelType)
        {
            var viewDataType = viewData.GetType();

            // if viewData is not generic then it is a simple ViewDataDictionary instance and its
            // Model property is of type 'object' and will accept anything, so it is safe to use
            // viewData
            if (viewDataType.IsGenericType == false)
                return viewData;

            // ensure it is the proper generic type
            var def = viewDataType.GetGenericTypeDefinition();
            if (def != typeof(ViewDataDictionary<>))
                throw new Exception("Could not map viewData of type \"" + viewDataType.FullName + "\".");

            // get the viewData model type and compare with the actual view model type:
            // viewData is ViewDataDictionary<viewDataModelType> and we will want to assign an
            // object of type modelType to the Model property of type viewDataModelType, we
            // need to check whether that is possible
            var viewDataModelType = viewDataType.GenericTypeArguments[0];

            if (viewDataModelType.IsAssignableFrom(modelType))
                return viewData;

            // if not possible then we need to create a new ViewDataDictionary
            var nViewDataType = typeof(ViewDataDictionary<>).MakeGenericType(modelType);
            var tViewData = new ViewDataDictionary(viewData) { Model = null }; // temp view data to copy values
            var nViewData = (ViewDataDictionary)Activator.CreateInstance(nViewDataType, tViewData);
            return nViewData;
        }

        /// <summary>
        /// This will detect the end /body tag and insert the preview badge if in preview mode
        /// </summary>
        /// <param name="value"></param>
        public override void WriteLiteral(object value)
        {
            // filter / add preview banner
            if (Response.ContentType.InvariantEquals("text/html")) // ASP.NET default value
            {
                if (UmbracoContext.Current.IsDebug || UmbracoContext.Current.InPreviewMode)
                {
                    var text = value.ToString();
                    var pos = text.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

                    if (pos > -1)
                    {
                        string markupToInject;

                        if (UmbracoContext.Current.InPreviewMode)
                        {
                            // creating previewBadge markup
                            markupToInject =
                                String.Format(UmbracoConfig.For.UmbracoSettings().Content.PreviewBadge,
                                    IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                                    IOHelper.ResolveUrl(SystemDirectories.UmbracoClient),
                                    Server.UrlEncode(UmbracoContext.Current.HttpContext.Request.Path));
                        }
                        else
                        {
                            // creating mini-profiler markup
                            markupToInject = Html.RenderProfiler().ToHtmlString();
                        }

                        var sb = new StringBuilder(text);
                        sb.Insert(pos, markupToInject);

                        base.WriteLiteral(sb.ToString());
                        return;
                    }
                }
            }

            base.WriteLiteral(value);


        }

        public HelperResult RenderSection(string name, Func<dynamic, HelperResult> defaultContents)
        {
            return WebViewPageExtensions.RenderSection(this, name, defaultContents);
        }

        public HelperResult RenderSection(string name, HelperResult defaultContents)
        {
            return WebViewPageExtensions.RenderSection(this, name, defaultContents);
        }

        public HelperResult RenderSection(string name, string defaultContents)
        {
            return WebViewPageExtensions.RenderSection(this, name, defaultContents);
        }
        
        public HelperResult RenderSection(string name, IHtmlString defaultContents)
        {
            return WebViewPageExtensions.RenderSection(this, name, defaultContents);
        }
    }
}
