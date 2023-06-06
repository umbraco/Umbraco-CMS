using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface ITemplatePresentationFactory
{
    Task<TemplateResponseModel> CreateTemplateResponseModelAsync(ITemplate template);
}
