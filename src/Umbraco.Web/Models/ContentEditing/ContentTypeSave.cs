using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class ContentTypeSave : ContentTypeBasic, IValidatableObject
    {
        public ContentTypeSave()
        {
            //initialize collections so at least their never null
            Groups = new List<PropertyGroupBasic<PropertyTypeBasic>>();
            AllowedContentTypes = new List<int>();
            CompositeContentTypes = new List<string>();
        }

        //Compositions
        [DataMember(Name = "compositeContentTypes")]
        public IEnumerable<string> CompositeContentTypes { get; set; }

        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        [DataMember(Name = "allowAsRoot")]
        public bool AllowAsRoot { get; set; }

        /// <summary>
        /// The list of allowed templates to assign (template alias)
        /// </summary>
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<string> AllowedTemplates { get; set; }

        //Allowed child types
        [DataMember(Name = "allowedContentTypes")]
        public IEnumerable<int> AllowedContentTypes { get; set; }

        /// <summary>
        /// The default template to assign (template alias)
        /// </summary>
        [DataMember(Name = "defaultTemplate")]
        public string DefaultTemplate { get; set; }

        //Tabs
        [DataMember(Name = "groups")]
        public IEnumerable<PropertyGroupBasic<PropertyTypeBasic>> Groups { get; set; }

        /// <summary>
        /// Custom validation
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AllowedTemplates.Any(x => x.IsNullOrWhiteSpace()))
                yield return new ValidationResult("Template value cannot be null", new[] {"AllowedTemplates"});

            if (CompositeContentTypes.Any(x => x.IsNullOrWhiteSpace()))
                yield return new ValidationResult("Composite Content Type value cannot be null", new[] { "CompositeContentTypes" });

            var duplicateGroups = Groups.GroupBy(x => x.Name).Where(x => x.Count() > 1).ToArray();
            if (duplicateGroups.Any())
            {
                //we need to return the field name with an index so it's wired up correctly
                var firstIndex = Groups.IndexOf(duplicateGroups.First().First());
                yield return new ValidationResult("Duplicate group names not allowed", new[]
                {
                    string.Format("Groups[{0}].Name", firstIndex)
                });
            }
            
            var duplicateProperties = Groups.SelectMany(x => x.Properties).Where(x => x.Inherited == false).GroupBy(x => x.Alias).Where(x => x.Count() > 1).ToArray();
            if (duplicateProperties.Any())
            {
                //we need to return the field name with an index so it's wired up correctly
                var firstProperty = duplicateProperties.First().First();
                var propertyGroup = Groups.Single(x => x.Properties.Contains(firstProperty));
                var groupIndex = Groups.IndexOf(propertyGroup);
                var propertyIndex = propertyGroup.Properties.IndexOf(firstProperty);

                yield return new ValidationResult("Duplicate property aliases not allowed", new[]
                {
                    string.Format("Groups[{0}].Properties[{1}].Alias", groupIndex, propertyIndex)
                });
            }
            
        }
    }
}