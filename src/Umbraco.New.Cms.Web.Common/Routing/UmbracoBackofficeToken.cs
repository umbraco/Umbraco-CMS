using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Umbraco.New.Cms.Web.Common.Routing;

/// <summary>
/// Adds a custom template token for specifying backoffice route with attribute routing
/// </summary>
// Adapted from https://stackoverflow.com/questions/68911881/asp-net-core-api-add-custom-route-token-resolver
public class UmbracoBackofficeToken : IApplicationModelConvention
{
    private readonly string _umbracoPath;
    private readonly string _tokenRegex;

    public UmbracoBackofficeToken(string tokenName, string umbracoPath)
    {
        _umbracoPath = umbracoPath;
        _tokenRegex = $@"(\[{tokenName}])(?<!\[\1(?=]))";
    }

    public void Apply(ApplicationModel application)
    {
        foreach (ControllerModel controller in application.Controllers)
        {
            UpdateSelectors(controller.Selectors, _umbracoPath);
            UpdateSelectors(controller.Actions.SelectMany(actionModel => actionModel.Selectors), _umbracoPath);
        }
    }

    private void UpdateSelectors(IEnumerable<SelectorModel> selectors, string tokenValue)
    {
        foreach (SelectorModel selector in selectors.Where(s => s.AttributeRouteModel is not null))
        {
            // We just checked that AttributeRouteModel is not null, so silence the nullable warning
            selector.AttributeRouteModel!.Template = InsertTokenValue(selector.AttributeRouteModel.Template, tokenValue);
            selector.AttributeRouteModel.Name = InsertTokenValue(selector.AttributeRouteModel.Name, tokenValue);
        }
    }

    private string? InsertTokenValue(string? template, string tokenValue)
        => template is null ? template : Regex.Replace(template, _tokenRegex, tokenValue);
}
