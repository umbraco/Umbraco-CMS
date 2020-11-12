using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.ModelBinders;

namespace Umbraco.Web.Common.AspNetCore
{

    public abstract class UmbracoViewPage : UmbracoViewPage<IPublishedContent>
    {

    }

    public abstract class UmbracoViewPage<TModel> : RazorPage<TModel>
    {

        private IUmbracoContext _umbracoContext;
        private IUmbracoContextAccessor UmbracoContextAccessor => Context.RequestServices.GetRequiredService<IUmbracoContextAccessor>();
        private GlobalSettings GlobalSettings => Context.RequestServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
        private ContentSettings ContentSettings => Context.RequestServices.GetRequiredService<IOptions<ContentSettings>>().Value;
        private IProfilerHtml ProfilerHtml => Context.RequestServices.GetRequiredService<IProfilerHtml>();
        private IIOHelper IOHelper => Context.RequestServices.GetRequiredService<IIOHelper>();
        private ContentModelBinder ContentModelBinder => new ContentModelBinder();

        protected IUmbracoContext UmbracoContext => _umbracoContext ??= UmbracoContextAccessor.UmbracoContext;


        public override void Write(object value)
        {
            if (value is IHtmlEncodedString htmlEncodedString)
            {
                base.WriteLiteral(htmlEncodedString.ToHtmlString());
            }
            else
            {
                base.Write(value);
            }
        }

        public override void WriteLiteral(object value)
        {
            // filter / add preview banner
            if (Context.Response.ContentType.InvariantEquals("text/html")) // ASP.NET default value
            {
                if (UmbracoContext.IsDebug || UmbracoContext.InPreviewMode)
                {
                    var text = value.ToString();
                    var pos = text.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

                    if (pos > -1)
                    {
                        string markupToInject;

                        if (UmbracoContext.InPreviewMode)
                        {
                            // creating previewBadge markup
                            markupToInject =
                                string.Format(ContentSettings.PreviewBadge,
                                    IOHelper.ResolveUrl(GlobalSettings.UmbracoPath),
                                    Context.Request.GetEncodedUrl(),
                                    UmbracoContext.PublishedRequest.PublishedContent.Id);
                        }
                        else
                        {
                            // creating mini-profiler markup
                            markupToInject = ProfilerHtml.Render();
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

        // maps model
        protected async Task SetViewDataAsync(ViewDataDictionary viewData)
        {
            // capture the model before we tinker with the viewData
            var viewDataModel = viewData.Model;

            // map the view data (may change its type, may set model to null)
            viewData = MapViewDataDictionary(viewData, typeof (TModel));

            // bind the model
            var bindingContext = new DefaultModelBindingContext();
            await ContentModelBinder.BindModelAsync(bindingContext, viewDataModel, typeof (TModel));

            viewData.Model = bindingContext.Result.Model;

            // set the view data
            ViewData = (ViewDataDictionary<TModel>) viewData;
        }



        // viewData is the ViewDataDictionary (maybe <TModel>) that we have
        // modelType is the type of the model that we need to bind to
        //
        // figure out whether viewData can accept modelType else replace it
        //
        private static ViewDataDictionary MapViewDataDictionary(ViewDataDictionary viewData, Type modelType)
        {
            var viewDataType = viewData.GetType();


            if (viewDataType.IsGenericType)
            {
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
            }

            // if not possible or it is not generic then we need to create a new ViewDataDictionary
            var nViewDataType = typeof(ViewDataDictionary<>).MakeGenericType(modelType);
            var tViewData = new ViewDataDictionary(viewData) { Model = null }; // temp view data to copy values
            var nViewData = (ViewDataDictionary)Activator.CreateInstance(nViewDataType, tViewData);
            return nViewData;
        }

        public HtmlString RenderSection(string name, HtmlString defaultContents)
        {
            return RazorPageExtensions.RenderSection(this, name, defaultContents);
        }

        public HtmlString RenderSection(string name, string defaultContents)
        {
            return RazorPageExtensions.RenderSection(this, name, defaultContents);
        }

    }
}
