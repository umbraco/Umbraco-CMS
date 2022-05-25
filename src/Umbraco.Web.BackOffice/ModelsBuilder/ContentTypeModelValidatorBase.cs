using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

public abstract class ContentTypeModelValidatorBase<TModel, TProperty> : EditorValidator<TModel>
    where TModel : ContentTypeSave<TProperty>
    where TProperty : PropertyTypeBasic
{
    private readonly IOptions<ModelsBuilderSettings> _config;

    public ContentTypeModelValidatorBase(IOptions<ModelsBuilderSettings> config) => _config = config;

    protected override IEnumerable<ValidationResult> Validate(TModel model)
    {
        // don't do anything if we're not enabled
        if (_config.Value.ModelsMode == ModelsMode.Nothing)
        {
            yield break;
        }

        // list of reserved/disallowed aliases for content/media/member types - more can be added as the need arises
        var reservedModelAliases = new[] { "system" };
        if (reservedModelAliases.Contains(model.Alias, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult($"The model alias {model.Alias} is a reserved term and cannot be used", new[] { "Alias" });
        }

        TProperty[] properties = model.Groups.SelectMany(x => x.Properties)
            .Where(x => x.Inherited == false)
            .ToArray();

        foreach (TProperty prop in properties)
        {
            PropertyGroupBasic<TProperty> propertyGroup = model.Groups.Single(x => x.Properties.Contains(prop));

            if (model.Alias.ToLowerInvariant() == prop.Alias.ToLowerInvariant())
            {
                string[] memberNames =
                {
                    $"Groups[{model.Groups.IndexOf(propertyGroup)}].Properties[{propertyGroup.Properties.IndexOf(prop)}].Alias"
                };

                yield return new ValidationResult(
                    string.Format(
                        "With Models Builder enabled, you can't have a property with a the alias \"{0}\" when the content type alias is also \"{0}\".",
                        prop.Alias),
                    memberNames);
            }

            // we need to return the field name with an index so it's wired up correctly
            var groupIndex = model.Groups.IndexOf(propertyGroup);
            var propertyIndex = propertyGroup.Properties.IndexOf(prop);

            ValidationResult? validationResult = ValidateProperty(prop, groupIndex, propertyIndex);
            if (validationResult != null)
            {
                yield return validationResult;
            }
        }
    }

    private ValidationResult? ValidateProperty(PropertyTypeBasic property, int groupIndex, int propertyIndex)
    {
        // don't let them match any properties or methods in IPublishedContent
        // TODO: There are probably more!
        var reservedProperties = typeof(IPublishedContent).GetProperties().Select(x => x.Name).ToArray();
        var reservedMethods = typeof(IPublishedContent).GetMethods().Select(x => x.Name).ToArray();

        var alias = property.Alias;

        if (reservedProperties.InvariantContains(alias) || reservedMethods.InvariantContains(alias))
        {
            string[] memberNames = { $"Groups[{groupIndex}].Properties[{propertyIndex}].Alias" };

            return new ValidationResult(
                $"The alias {alias} is a reserved term and cannot be used",
                memberNames);
        }

        return null;
    }
}
