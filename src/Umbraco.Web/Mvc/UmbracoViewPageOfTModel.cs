﻿using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Represents the properties and methods that are needed in order to render an Umbraco view.
    /// </summary>
    public abstract class UmbracoViewPage<TModel> : WebViewPage<TModel>
    {
        private UmbracoContext _umbracoContext;
        private UmbracoHelper _helper;

        /// <summary>
        /// Gets or sets the database context.
        /// </summary>
        public ServiceContext Services { get; set; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get; set; }

        // TODO: previously, Services and ApplicationCache would derive from UmbracoContext.Application, which
        // was an ApplicationContext - so that everything derived from UmbracoContext.
        // UmbracoContext is fetched from the data tokens, thus allowing the view to be rendered with a
        // custom context and NOT the Current.UmbracoContext - eg outside the normal Umbraco routing
        // process.
        // leaving it as-it for the time being but really - the UmbracoContext should be injected just
        // like the Services & ApplicationCache properties, and have a setter for those special weird
        // cases.

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext => _umbracoContext
            ?? (_umbracoContext = ViewContext.GetUmbracoContext() ?? Current.UmbracoContext);

        /// <summary>
        /// Gets the public content request.
        /// </summary>
        internal PublishedRequest PublishedRequest
        {
            get
            {
                const string token = Core.Constants.Web.PublishedDocumentRequestDataToken;

                // we should always try to return the object from the data tokens just in case its a custom object and not
                // the one from UmbracoContext. Fallback to UmbracoContext if necessary.

                // try view context
                if (ViewContext.RouteData.DataTokens.ContainsKey(token))
                    return (PublishedRequest) ViewContext.RouteData.DataTokens.GetRequiredObject(token);

                // child action, try parent view context
                if (ViewContext.IsChildAction && ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey(token))
                    return (PublishedRequest) ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject(token);

                // fallback to UmbracoContext
                return UmbracoContext.PublishedRequest;
            }
        }

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get
            {
                if (_helper != null) return _helper;

                var model = ViewData.Model;
                var content = model as IPublishedContent;
                if (content == null && model is IContentModel)
                    content = ((IContentModel) model).Content;

                _helper = Current.UmbracoHelper;

                if (content != null)
                    _helper.AssignedContentItem = content;
                
                return _helper;
            }
        }

        /// <summary>
        /// Gets the membership helper.
        /// </summary>
        public MembershipHelper Members => Umbraco.MembershipHelper;

        protected UmbracoViewPage()
            : this(
                Current.Factory.GetInstance<ServiceContext>(),
                Current.Factory.GetInstance<AppCaches>()
            )
        {
        }

        protected UmbracoViewPage(ServiceContext services, AppCaches appCaches)
        {
            Services = services;
            AppCaches = appCaches;
        }

        // view logic below:

        /// <summary>
        /// Ensure that the current view context is added to the route data tokens so we can extract it if we like
        /// </summary>
        /// <remarks>
        /// Currently this is required by mvc macro engines
        /// </remarks>
        protected override void InitializePage()
        {
            base.InitializePage();

            if (ViewContext.IsChildAction) return;

            // this is used purely for partial view macros that contain forms and mostly
            // just when rendered within the RTE - this should already be set with the
            // EnsurePartialViewMacroViewContextFilterAttribute
            if (ViewContext.RouteData.DataTokens.ContainsKey(Constants.DataTokenCurrentViewContext) == false)
                ViewContext.RouteData.DataTokens.Add(Constants.DataTokenCurrentViewContext, ViewContext);
        }

        // maps model
        protected override void SetViewData(ViewDataDictionary viewData)
        {
            // capture the model before we tinker with the viewData
            var viewDataModel = viewData.Model;

            // map the view data (may change its type, may set model to null)
            viewData = MapViewDataDictionary(viewData, typeof (TModel));

            // bind the model
            viewData.Model = ContentModelBinder.BindModel(viewDataModel, typeof (TModel));

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
                if (Current.UmbracoContext.IsDebug || Current.UmbracoContext.InPreviewMode)
                {
                    var text = value.ToString();
                    var pos = text.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

                    if (pos > -1)
                    {
                        string markupToInject;

                        if (Current.UmbracoContext.InPreviewMode)
                        {
                            // creating previewBadge markup
                            markupToInject =
                                string.Format(Current.Configs.Settings().Content.PreviewBadge,
                                    IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                                    Server.UrlEncode(Current.UmbracoContext.HttpContext.Request.Url?.PathAndQuery));
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
