using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Used to create <see cref="UmbracoRouteValues" />
/// </summary>
public class UmbracoRouteValuesFactory : IUmbracoRouteValuesFactory
{
    private readonly IControllerActionSearcher _controllerActionSearcher;
    private readonly Lazy<ControllerActionDescriptor> _defaultControllerDescriptor;
    private readonly Lazy<string> _defaultControllerName;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UmbracoFeatures _umbracoFeatures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRouteValuesFactory" /> class.
    /// </summary>
    public UmbracoRouteValuesFactory(
        IOptions<UmbracoRenderingDefaultsOptions> renderingDefaults,
        IShortStringHelper shortStringHelper,
        UmbracoFeatures umbracoFeatures,
        IControllerActionSearcher controllerActionSearcher,
        IPublishedRouter publishedRouter)
    {
        _shortStringHelper = shortStringHelper;
        _umbracoFeatures = umbracoFeatures;
        _controllerActionSearcher = controllerActionSearcher;
        _publishedRouter = publishedRouter;
        _defaultControllerName = new Lazy<string>(() =>
            ControllerExtensions.GetControllerName(renderingDefaults.Value.DefaultControllerType));
        _defaultControllerDescriptor = new Lazy<ControllerActionDescriptor>(() =>
        {
            ControllerActionDescriptor? descriptor = _controllerActionSearcher.Find<IRenderController>(
                new DefaultHttpContext(), // this actually makes no difference for this method
                DefaultControllerName,
                UmbracoRouteValues.DefaultActionName);

            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    $"No controller/action found by name {DefaultControllerName}.{UmbracoRouteValues.DefaultActionName}");
            }

            return descriptor;
        });
    }

    /// <summary>
    ///     Gets the default controller name
    /// </summary>
    protected string DefaultControllerName => _defaultControllerName.Value;

    /// <inheritdoc />
    public async Task<UmbracoRouteValues> CreateAsync(HttpContext httpContext, IPublishedRequest request)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        string? customActionName = GetTemplateName(request);

        // The default values for the default controller/action
        var def = new UmbracoRouteValues(
            request,
            _defaultControllerDescriptor.Value,
            customActionName);

        def = CheckHijackedRoute(httpContext, def, out var hasHijackedRoute);

        def = await CheckNoTemplateAsync(httpContext, def, hasHijackedRoute);

        return def;
    }

    /// <summary>
    ///     Check if the route is hijacked and return new route values
    /// </summary>
    private UmbracoRouteValues CheckHijackedRoute(
        HttpContext httpContext, UmbracoRouteValues def, out bool hasHijackedRoute)
    {
        IPublishedRequest request = def.PublishedRequest;

        var customControllerName = request.PublishedContent?.ContentType?.Alias;
        if (customControllerName != null)
        {
            ControllerActionDescriptor? descriptor =
                _controllerActionSearcher.Find<IRenderController>(httpContext, customControllerName, def.TemplateName);
            if (descriptor != null)
            {
                hasHijackedRoute = true;

                return new UmbracoRouteValues(
                    request,
                    descriptor,
                    def.TemplateName);
            }
        }

        hasHijackedRoute = false;
        return def;
    }

    /// <summary>
    ///     Special check for when no template or hijacked route is done which needs to re-run through the routing pipeline
    ///     again for last chance finders
    /// </summary>
    private async Task<UmbracoRouteValues> CheckNoTemplateAsync(
        HttpContext httpContext, UmbracoRouteValues def, bool hasHijackedRoute)
    {
        IPublishedRequest request = def.PublishedRequest;

        // Here we need to check if there is no hijacked route and no template assigned but there is a content item.
        // If this is the case we want to return a blank page.
        // We also check if templates have been disabled since if they are then we're allowed to render even though there's no template,
        // for example for json rendering in headless.
        if (request.HasPublishedContent()
            && !request.HasTemplate()
            && !_umbracoFeatures.Disabled.DisableTemplates
            && !hasHijackedRoute)
        {
            IPublishedContent? content = request.PublishedContent;

            // This is basically a 404 even if there is content found.
            // We then need to re-run this through the pipeline for the last
            // chance finders to work.
            // Set to null since we are telling it there is no content.
            request = await _publishedRouter.UpdateRequestAsync(request, null);

            if (request == null)
            {
                throw new InvalidOperationException(
                    $"The call to {nameof(IPublishedRouter.UpdateRequestAsync)} cannot return null");
            }

			string? customActionName = GetTemplateName(request);
			
            def = new UmbracoRouteValues(
                request,
                def.ControllerActionDescriptor,
                customActionName);

            // if the content has changed, we must then again check for hijacked routes
            if (content != request.PublishedContent)
            {
                def = CheckHijackedRoute(httpContext, def, out _);
            }
        }

        return def;
    }
	
	private string? GetTemplateName(IPublishedRequest request)
    {
        // check that a template is defined), if it doesn't and there is a hijacked route it will just route
        // to the index Action
        if (request.HasTemplate())
        {
            // the template Alias should always be already saved with a safe name.
            // if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
            // with the action name attribute.
            return request.GetTemplateAlias()?.Split('.')[0].ToSafeAlias(_shortStringHelper);
        }

        return null;
    }
}
