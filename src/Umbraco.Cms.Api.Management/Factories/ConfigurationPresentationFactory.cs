﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class ConfigurationPresentationFactory : IConfigurationPresentationFactory
{
    private readonly IReservedFieldNamesService _reservedFieldNamesService;
    private readonly UmbracoFeatures _umbracoFeatures;
    private readonly DataTypesSettings _dataTypesSettings;
    private readonly ContentSettings _contentSettings;
    private readonly SegmentSettings _segmentSettings;

    public ConfigurationPresentationFactory(
        IReservedFieldNamesService reservedFieldNamesService,
        IOptions<ContentSettings> contentSettings,
        IOptions<SegmentSettings> segmentSettings,
        IOptions<DataTypesSettings> dataTypesSettings,
        UmbracoFeatures umbracoFeatures)
    {
        _reservedFieldNamesService = reservedFieldNamesService;
        _umbracoFeatures = umbracoFeatures;
        _dataTypesSettings = dataTypesSettings.Value;
        _contentSettings = contentSettings.Value;
        _segmentSettings = segmentSettings.Value;
    }

    [Obsolete("Use the constructor with all dependencies")]
    public ConfigurationPresentationFactory(
        IReservedFieldNamesService reservedFieldNamesService,
        IOptions<ContentSettings> contentSettings,
        IOptions<SegmentSettings> segmentSettings)
    : this(
        reservedFieldNamesService,
        contentSettings,
        segmentSettings,
        StaticServiceProvider.Instance.GetRequiredService<IOptions<DataTypesSettings>>(),
        StaticServiceProvider.Instance.GetRequiredService<UmbracoFeatures>()
            )
    {
    }

    public DocumentConfigurationResponseModel CreateDocumentConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            AllowEditInvariantFromNonDefault = _contentSettings.AllowEditInvariantFromNonDefault,
            AllowNonExistingSegmentsCreation = _segmentSettings.AllowCreation,
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    public DocumentTypeConfigurationResponseModel CreateDocumentTypeConfigurationResponseModel() =>
        new()
        {
            DataTypesCanBeChanged = _dataTypesSettings.CanBeChanged,
            DisableTemplates = _umbracoFeatures.Disabled.DisableTemplates,
            UseSegments = _segmentSettings.Enabled,
            ReservedFieldNames = _reservedFieldNamesService.GetDocumentReservedFieldNames(),
        };

    public MemberConfigurationResponseModel CreateMemberConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    public MemberTypeConfigurationResponseModel CreateMemberTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMemberReservedFieldNames(),
        };

    public MediaConfigurationResponseModel CreateMediaConfigurationResponseModel() =>
        new()
        {
            DisableDeleteWhenReferenced = _contentSettings.DisableDeleteWhenReferenced,
            DisableUnpublishWhenReferenced = _contentSettings.DisableUnpublishWhenReferenced,
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };

    public MediaTypeConfigurationResponseModel CreateMediaTypeConfigurationResponseModel() =>
        new()
        {
            ReservedFieldNames = _reservedFieldNamesService.GetMediaReservedFieldNames(),
        };
}
