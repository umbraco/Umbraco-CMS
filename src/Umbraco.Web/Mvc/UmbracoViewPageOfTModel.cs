using System;
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
                //using the UmbracoContext.Current.
                //we will fallback to the singleton if necessary.
                if (ViewContext.RouteData.DataTokens.ContainsKey("umbraco-context"))
                {
                    return (UmbracoContext)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context");
                }
                //next check if it is a child action and see if the parent has it set in data tokens
                if (ViewContext.IsChildAction)
                {
                    if (ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey("umbraco-context"))
                    {
                        return (UmbracoContext)ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context");
                    }
                }

                //lastly, we will use the singleton, the only reason this should ever happen is is someone is rendering a page that inherits from this
                //class and are rendering it outside of the normal Umbraco routing process. Very unlikely.
                return UmbracoContext.Current;
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
                if (ViewContext.RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
                {
                    return (PublishedContentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
                }
                //next check if it is a child action and see if the parent has it set in data tokens
                if (ViewContext.IsChildAction)
                {
                    if (ViewContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey("umbraco-doc-request"))
                    {
                        return (PublishedContentRequest)ViewContext.ParentActionViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
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
                if (ViewContext.RouteData.DataTokens.ContainsKey(Constants.DataTokenCurrentViewContext) == false)
                {
                    ViewContext.RouteData.DataTokens.Add(Constants.DataTokenCurrentViewContext, ViewContext);
                }
            }

        }

        // maps model
        protected override void SetViewData(ViewDataDictionary viewData)
        {
            // if view data contains no model, nothing to do
            var source = viewData.Model;
            if (source == null)
            {
                base.SetViewData(viewData);
                return;
            }

            // get the type of the view data model (what we have)
            // get the type of this view model (what we want)
            var sourceType = source.GetType();
            var targetType = typeof (TModel);

            // it types already match, nothing to do
            if (sourceType.Inherits<TModel>()) // includes ==
            {
                base.SetViewData(viewData);
                return;
            }

            // try to grab the content
            // if no content is found, return, nothing we can do
            var sourceContent = source as IPublishedContent; // check if what we have is an IPublishedContent
            if (sourceContent == null && sourceType.Implements<IRenderModel>())
            {
                // else check if it's an IRenderModel => get the content
                sourceContent = ((IRenderModel)source).Content;
            }
            if (sourceContent == null)
            {
                // else check if we can convert it to a content
                var attempt = source.TryConvertTo<IPublishedContent>();
                if (attempt.Success) sourceContent = attempt.Result;
            }

            var ok = sourceContent != null;
            if (sourceContent != null)
            {
                // try to grab the culture
                // using context's culture by default
                var culture = UmbracoContext.PublishedContentRequest.Culture;
                var sourceRenderModel = source as RenderModel;
                if (sourceRenderModel != null)
                    culture = sourceRenderModel.CurrentCulture;

                // reassign the model depending on its type
                if (targetType.Implements<IPublishedContent>())
                {
                    // it TModel implements IPublishedContent then use the content
                    // provided that the content is of the proper type
                    if ((sourceContent is TModel) == false)
                        throw new InvalidCastException(string.Format("Cannot cast source content type {0} to view model type {1}.",
                            sourceContent.GetType(), targetType));
                    viewData.Model = sourceContent;
                }
                else if (targetType == typeof(RenderModel))
                {
                    // if TModel is a basic RenderModel just create it
                    viewData.Model = new RenderModel(sourceContent, culture);
                }
                else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(RenderModel<>))
                {
                    // if TModel is a strongly-typed RenderModel<> then create it
                    // provided that the content is of the proper type
                    var targetContentType = targetType.GetGenericArguments()[0];
                    if ((sourceContent.GetType().Inherits(targetContentType)) == false)
                        throw new InvalidCastException(string.Format("Cannot cast source content type {0} to view model content type {1}.",
                            sourceContent.GetType(), targetContentType));
                    viewData.Model = Activator.CreateInstance(targetType, sourceContent, culture);
                }
                else
                {
                    ok = false;
                }
            }

            if (ok == false)
            {
                // last chance : try to convert
                var attempt = source.TryConvertTo<TModel>();
                if (attempt.Success) viewData.Model = attempt.Result;
            }

            base.SetViewData(viewData);
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
