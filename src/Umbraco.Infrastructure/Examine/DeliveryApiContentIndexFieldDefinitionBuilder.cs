using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndexFieldDefinitionBuilder : IDeliveryApiContentIndexFieldDefinitionBuilder
{
    private readonly ContentIndexHandlerCollection _indexHandlers;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> _logger;
    private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;

    public DeliveryApiContentIndexFieldDefinitionBuilder(
        ContentIndexHandlerCollection indexHandlers,
        ILocalizationService localizationService,
        ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> logger,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
    {
        _indexHandlers = indexHandlers;
        _localizationService = localizationService;
        _logger = logger;
        _installDefaultDataSettings = installDefaultDataSettings;
    }

    public FieldDefinitionCollection Build()
    {
        var fieldDefinitions = new List<FieldDefinition>();

        AddRequiredFieldDefinitions(fieldDefinitions);
        AddContentIndexHandlerFieldDefinitions(fieldDefinitions);

        return new FieldDefinitionCollection(fieldDefinitions.ToArray());
    }

    // required field definitions go here
    // see also the field definitions in the Delivery API content index value set builder
    private void AddRequiredFieldDefinitions(ICollection<FieldDefinition> fieldDefinitions)
    {
        fieldDefinitions.Add(new("id", FieldDefinitionTypes.Integer));
        fieldDefinitions.Add(new("cultures", FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.IndexPathFieldName, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.NodeNameFieldName, FieldDefinitionTypes.Raw));
    }

    private void AddContentIndexHandlerFieldDefinitions(ICollection<FieldDefinition> fieldDefinitions)
    {
        var cultures = GetCultures();

        // add index fields from index handlers (selectors, filters, sorts)
        foreach (IContentIndexHandler handler in _indexHandlers)
        {
            IndexField[] fields = GetContentIndexHandlerFields(handler, cultures);

            foreach (IndexField field in fields)
            {
                if (fieldDefinitions.Any(fieldDefinition => fieldDefinition.Name.InvariantEquals(field.FieldName)))
                {
                    _logger.LogWarning("Duplicate field definitions found for field name {FieldName} among the index handlers - first one wins.", field.FieldName);
                    continue;
                }

                FieldDefinition fieldDefinition = CreateFieldDefinition(field);
                fieldDefinitions.Add(fieldDefinition);
            }
        }
    }

    private string[] GetCultures()
    {
        try
        {
            return _localizationService.GetAllLanguages().Select(language => language.IsoCode).ToArray();
        }
        catch (Exception)
        {
            // the field definition collection is built during boot, which may happen without an actual database (i.e. before install). if an exception occurs here,
            // we'll assume the database does not yet exist and continue using the default install culture (fallback to en-US).

            InstallDefaultDataSettings languageInstallDefaultDataSettings = _installDefaultDataSettings.Get(Constants.Configuration.NamedOptions.InstallDefaultData.Languages);
            return languageInstallDefaultDataSettings.InstallData is InstallDefaultDataOption.Values
                   && languageInstallDefaultDataSettings.Values.Any()
                ? languageInstallDefaultDataSettings.Values.ToArray()
                : new[] { "en-US" };
        }
    }

    // calculates the actual index fields to add for this index handler
    // - if a field varies by culture, we must add a field per culture to contain all the (potential) values
    // - if a field does not vary by culture, just add the field as-is
    private static IndexField[] GetContentIndexHandlerFields(IContentIndexHandler handler, string[] cultures) =>
        handler.GetFields().SelectMany(
            field => field.VariesByCulture
                ? cultures.Select(culture => new IndexField
                {
                    FieldName = CultureVariantIndexFieldName(field.FieldName, culture),
                    FieldType = field.FieldType,
                    VariesByCulture = field.VariesByCulture
                }).ToArray()
                : new[] { field }).ToArray();

    private static FieldDefinition CreateFieldDefinition(IndexField field)
    {
        var indexType = field.FieldType switch
        {
            FieldType.Date => FieldDefinitionTypes.DateTime,
            FieldType.Number => FieldDefinitionTypes.Integer,
            FieldType.StringRaw => FieldDefinitionTypes.Raw,
            FieldType.StringAnalyzed => FieldDefinitionTypes.FullText,
            FieldType.StringSortable => FieldDefinitionTypes.FullTextSortable,
            _ => throw new ArgumentOutOfRangeException(nameof(field.FieldType))
        };

        return new FieldDefinition(field.FieldName, indexType);
    }

    public static string CultureVariantIndexFieldName(string fieldName, string culture) => $"{fieldName}_{culture}";
}
