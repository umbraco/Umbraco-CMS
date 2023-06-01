﻿using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class TemplatePresentationFactory : ITemplatePresentationFactory
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _mapper;

    public TemplatePresentationFactory(
        ITemplateService templateService,
        IUmbracoMapper mapper)
    {
        _templateService = templateService;
        _mapper = mapper;
    }

    public async Task<TemplateResponseModel> CreateTemplateResponseModelAsync(ITemplate template)
    {
        TemplateResponseModel responseModel = _mapper.Map<TemplateResponseModel>(template)!;

        if (template.MasterTemplateAlias is not null)
        {
            ITemplate? parentTemplate = await _templateService.GetAsync(template.MasterTemplateAlias);
            responseModel.MasterTemplateId = parentTemplate?.Key;
        }

        return responseModel;
    }
}
