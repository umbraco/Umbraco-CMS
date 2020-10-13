using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.ModelsBuilder.Embedded.BackOffice
{
    public abstract class ContentTypeModelValidatorBase<TModel, TProperty> : EditorValidator<TModel>
        where TModel : ContentTypeSave<TProperty>
        where TProperty : PropertyTypeBasic
    {
        private readonly IOptions<ModelsBuilderSettings> _config;

        public ContentTypeModelValidatorBase(IOptions<ModelsBuilderSettings> config)
        {
            _config = config;
        }

        protected override IEnumerable<ValidationResult> Validate(TModel model)
        {
            //don't do anything if we're not enabled
            if (!_config.Value.Enable) yield break;

            var properties = model.Groups.SelectMany(x => x.Properties)
                .Where(x => x.Inherited == false)
                .ToArray();

            foreach (var prop in properties)
            {
                var propertyGroup = model.Groups.Single(x => x.Properties.Contains(prop));

                if (model.Alias.ToLowerInvariant() == prop.Alias.ToLowerInvariant())
                    yield return new ValidationResult(string.Format("With Models Builder enabled, you can't have a property with a the alias \"{0}\" when the content type alias is also \"{0}\".", prop.Alias), new[]
                    {
                        $"Groups[{model.Groups.IndexOf(propertyGroup)}].Properties[{propertyGroup.Properties.IndexOf(prop)}].Alias"
                    });

                //we need to return the field name with an index so it's wired up correctly
                var groupIndex = model.Groups.IndexOf(propertyGroup);
                var propertyIndex = propertyGroup.Properties.IndexOf(prop);

                var validationResult = ValidateProperty(prop, groupIndex, propertyIndex);
                if (validationResult != null)
                    yield return validationResult;
            }
        }

        private ValidationResult ValidateProperty(PropertyTypeBasic property, int groupIndex, int propertyIndex)
        {
            //don't let them match any properties or methods in IPublishedContent
            //TODO: There are probably more!
            var reservedProperties = typeof(IPublishedContent).GetProperties().Select(x => x.Name).ToArray();
            var reservedMethods = typeof(IPublishedContent).GetMethods().Select(x => x.Name).ToArray();

            var alias = property.Alias;

            if (reservedProperties.InvariantContains(alias) || reservedMethods.InvariantContains(alias))
                return new ValidationResult(
                    $"The alias {alias} is a reserved term and cannot be used", new[]
                    {
                        $"Groups[{groupIndex}].Properties[{propertyIndex}].Alias"
                    });

            return null;
        }
    }
}
