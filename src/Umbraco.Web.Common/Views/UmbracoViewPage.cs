using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.Views;

public abstract class UmbracoViewPage : UmbracoViewPage<IPublishedContent>
{
}

public abstract class UmbracoViewPage<TModel> : RazorPage<TModel>
{
    private UmbracoHelper? _helper;

    /// <summary>
    ///     Gets the Umbraco helper.
    /// </summary>
    public UmbracoHelper Umbraco
    {
        get
        {
            if (_helper != null)
            {
                return _helper;
            }

            _helper = Context.RequestServices.GetRequiredService<UmbracoHelper>();

            TModel model = ViewData.Model;
            var content = model as IPublishedContent;

            if (content is null && model is IContentModel contentModel)
            {
                content = contentModel.Content;
            }

            content ??= UmbracoContext?.PublishedRequest?.PublishedContent;

            if (content is not null)
            {
                _helper.AssignedContentItem = content;
            }

            return _helper;
        }
    }

    /// <inheritdoc />
    public override ViewContext ViewContext
    {
        get => base.ViewContext;
        set
        {
            // Here we do the magic model swap
            ViewContext ctx = value;
            ctx.ViewData = BindViewData(
                ctx.HttpContext.RequestServices.GetRequiredService<ContentModelBinder>(),
                ctx.ViewData);
            base.ViewContext = ctx;
        }
    }

    /// <summary>
    ///     Gets the <see cref="IUmbracoContext" />
    /// </summary>
    protected IUmbracoContext? UmbracoContext
    {
        get
        {
            if (!UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
            {
                return null;
            }

            return umbracoContext;
        }
    }

    private IUmbracoContextAccessor UmbracoContextAccessor =>
        Context.RequestServices.GetRequiredService<IUmbracoContextAccessor>();

    private GlobalSettings GlobalSettings =>
        Context.RequestServices.GetRequiredService<IOptions<GlobalSettings>>().Value;

    private ContentSettings ContentSettings =>
        Context.RequestServices.GetRequiredService<IOptions<ContentSettings>>().Value;

    private IProfilerHtml ProfilerHtml => Context.RequestServices.GetRequiredService<IProfilerHtml>();

    private IHostingEnvironment HostingEnvironment => Context.RequestServices.GetRequiredService<IHostingEnvironment>();

    /// <inheritdoc />
    public override void Write(object? value)
    {
        if (value is IHtmlEncodedString htmlEncodedString)
        {
            WriteLiteral(htmlEncodedString.ToHtmlString());
        }
        else if (value is TagHelperOutput tagHelperOutput)
        {
            WriteUmbracoContent(tagHelperOutput);
            base.Write(value);
        }
        else
        {
            base.Write(value);
        }
    }

    public void WriteUmbracoContent(TagHelperOutput tagHelperOutput)
    {
        // filter / add preview banner
        // ASP.NET default value is text/html
        if (Context.Response?.ContentType?.InvariantContains("text/html") ?? false)
        {
            if (((UmbracoContext?.IsDebug ?? false) || (UmbracoContext?.InPreviewMode ?? false))
                && tagHelperOutput.TagName != null
                && tagHelperOutput.TagName.Equals("body", StringComparison.InvariantCultureIgnoreCase))
            {
                string markupToInject;

                if (UmbracoContext.InPreviewMode)
                {
                    // creating previewBadge markup
                    markupToInject =
                        string.Format(
                            ContentSettings.PreviewBadge,
                            HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoPath),
                            Context.Request.GetEncodedUrl(),
                            UmbracoContext.PublishedRequest?.PublishedContent?.Id);
                }
                else
                {
                    // creating mini-profiler markup
                    markupToInject = ProfilerHtml.Render();
                }

                tagHelperOutput.Content.AppendHtml(markupToInject);
            }
        }
    }

    /// <summary>
    ///     Renders a section with default content if the section isn't defined
    /// </summary>
    public HtmlString? RenderSection(string name, string defaultContents) =>
        RazorPageExtensions.RenderSection(this, name, defaultContents);

    /// <summary>
    ///     Renders a section with default content if the section isn't defined
    /// </summary>
    public HtmlString? RenderSection(string name, HtmlString defaultContents) =>
        RazorPageExtensions.RenderSection(this, name, defaultContents);

    /// <summary>
    ///     Dynamically binds the incoming <see cref="ViewDataDictionary" /> to the required
    ///     <see cref="ViewDataDictionary{TModel}" />
    /// </summary>
    /// <remarks>
    ///     This is used in order to provide the ability for an Umbraco view to either have a model of type
    ///     <see cref="IContentModel" /> or <see cref="IPublishedContent" />. This will use the
    ///     <see cref="ContentModelBinder" /> to bind the models
    ///     to the correct output type.
    /// </remarks>
    protected ViewDataDictionary BindViewData(ContentModelBinder contentModelBinder, ViewDataDictionary? viewData)
    {
        if (contentModelBinder is null)
        {
            throw new ArgumentNullException(nameof(contentModelBinder));
        }

        if (viewData is null)
        {
            throw new ArgumentNullException(nameof(viewData));
        }

        // check if it's already the correct type and continue if it is
        if (viewData is ViewDataDictionary<TModel> vdd)
        {
            return vdd;
        }

        // Here we hand the default case where we know the incoming model is ContentModel and the
        // outgoing model is IPublishedContent. This is a fast conversion that doesn't require doing the full
        // model binding, allocating classes, etc...
        if (viewData.ModelMetadata.ModelType == typeof(ContentModel)
            && typeof(TModel) == typeof(IPublishedContent))
        {
            var contentModel = (ContentModel?)viewData.Model;
            viewData.Model = contentModel?.Content;
            return viewData;
        }

        // capture the model before we tinker with the viewData
        var viewDataModel = viewData.Model;

        // map the view data (may change its type, may set model to null)
        viewData = MapViewDataDictionary(viewData, typeof(TModel));

        // bind the model
        var bindingContext = new DefaultModelBindingContext();
        contentModelBinder.BindModel(bindingContext, viewDataModel, typeof(TModel));

        viewData!.Model = bindingContext.Result.Model;

        // return the new view data
        return (ViewDataDictionary<TModel>)viewData;
    }

    // viewData is the ViewDataDictionary (maybe <TModel>) that we have
    // modelType is the type of the model that we need to bind to
    // figure out whether viewData can accept modelType else replace it
    private static ViewDataDictionary? MapViewDataDictionary(ViewDataDictionary viewData, Type modelType)
    {
        Type viewDataType = viewData.GetType();

        if (viewDataType.IsGenericType)
        {
            // ensure it is the proper generic type
            Type def = viewDataType.GetGenericTypeDefinition();
            if (def != typeof(ViewDataDictionary<>))
            {
                throw new Exception("Could not map viewData of type \"" + viewDataType.FullName + "\".");
            }

            // get the viewData model type and compare with the actual view model type:
            // viewData is ViewDataDictionary<viewDataModelType> and we will want to assign an
            // object of type modelType to the Model property of type viewDataModelType, we
            // need to check whether that is possible
            Type viewDataModelType = viewDataType.GenericTypeArguments[0];

            if (viewDataModelType != typeof(object) && viewDataModelType.IsAssignableFrom(modelType))
            {
                return viewData;
            }
        }

        // if not possible or it is not generic then we need to create a new ViewDataDictionary
        Type nViewDataType = typeof(ViewDataDictionary<>).MakeGenericType(modelType);
        var tViewData = new ViewDataDictionary(viewData) { Model = default(TModel) }; // temp view data to copy values
        var nViewData = (ViewDataDictionary?)Activator.CreateInstance(nViewDataType, tViewData);
        return nViewData;
    }
}
