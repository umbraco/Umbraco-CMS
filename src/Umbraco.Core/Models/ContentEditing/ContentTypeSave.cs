using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing
{
    /// <summary>
    /// Abstract model used to save content types
    /// </summary>
    [DataContract(Name = "contentType", Namespace = "")]
    public abstract class ContentTypeSave : ContentTypeBasic, IValidatableObject
    {
        protected ContentTypeSave()
        {
            AllowedContentTypes = new List<int>();
            CompositeContentTypes = new List<string>();
        }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<string> CompositeContentTypes { get; set; }

        [DataMember(Name = "allowAsRoot")]
        public bool AllowAsRoot { get; set; }

        //Allowed child types
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<int> AllowedContentTypes { get; set; }

        /// <summary>
        /// Custom validation
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CompositeContentTypes.Any(x => x.IsNullOrWhiteSpace()))
                yield return new ValidationResult("Composite Content Type value cannot be null", new[] { "CompositeContentTypes" });
        }
    }

    /// <summary>
    /// Abstract model used to save content types
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    [DataContract(Name = "contentType", Namespace = "")]
    public abstract class ContentTypeSave<TPropertyType> : ContentTypeSave
        where TPropertyType : PropertyTypeBasic
    {
        protected ContentTypeSave()
        {
            Groups = new List<PropertyGroupBasic<TPropertyType>>();
        }

        /// <summary>
        /// A rule for defining how a content type can be varied
        /// </summary>
        /// <remarks>
        /// This is only supported on document types right now but in the future it could be media types too
        /// </remarks>
        [DataMember(Name = "allowCultureVariant")]
        public bool AllowCultureVariant { get; set; }

        [DataMember(Name = "allowSegmentVariant")]
        public bool AllowSegmentVariant { get; set; }

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupBasic<TPropertyType>> Groups { get; set; }

        /// <summary>
        /// Custom validation
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var validationResult in base.Validate(validationContext))
            {
                yield return validationResult;
            }

            var duplicateGroups = Groups.GroupBy(x => x.Name).Where(x => x.Count() > 1).ToArray();
            if (duplicateGroups.Any())
            {
                //we need to return the field name with an index so it's wired up correctly
                var lastIndex = Groups.IndexOf(duplicateGroups.Last().Last());
                yield return new ValidationResult("Duplicate group names not allowed", new[]
                {
                    $"Groups[{lastIndex}].Name"
                });
            }

            var duplicateProperties = Groups.SelectMany(x => x.Properties).Where(x => x.Inherited == false).GroupBy(x => x.Alias).Where(x => x.Count() > 1).ToArray();
            if (duplicateProperties.Any())
            {
                //we need to return the field name with an index so it's wired up correctly
                var lastProperty = duplicateProperties.Last().Last();
                var propertyGroup = Groups.Single(x => x.Properties.Contains(lastProperty));

                yield return new ValidationResult("Duplicate property aliases not allowed: " + lastProperty.Alias, new[]
                {
                    $"Groups[{propertyGroup.SortOrder}].Properties[{lastProperty.SortOrder}].Alias"
                });
            }
        }
    }
}
