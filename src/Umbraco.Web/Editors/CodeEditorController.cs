using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class CodeEditorController : UmbracoApiController
    {
        public List<CodeEditorCompletion> GetModel(string modelName)
        {
            var completions = new List<CodeEditorCompletion>();

            //@inherits UmbracoViewPage<ContentModels.Products>
            //@using ContentModels = Umbraco.Web.PublishedModels;

            // Umbraco.Web.PublishedModels
            // Umbraco.Web.PublishedModels.Products

            // TODO: Use TypeFinder or TypeLoader


            // Find only the types/classes that have the ModelsBuilder attribute of
            // [Umbraco.Core.Models.PublishedContent.PublishedModelAttribute]
            var modelsBuilderTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(x => x.GetCustomAttributes()
                .Any(attr => attr.GetType().Equals(typeof(PublishedModelAttribute))));

            if(modelsBuilderTypes.Count() == 0)
            {
                // No ModelsBuilder stuff found
                return completions;
            }

            // Filter/select the type that contains/partial matches from modelName passed in
            // FullName Umbraco.Web.PublishedModels.Contact
            // Name: Contact
            // Namespace: Umbraco.Web.PublishedModels

            var modelType = modelsBuilderTypes.SingleOrDefault(x => x.Name.ToLowerInvariant() == modelName.ToLowerInvariant());
            if (modelType == null)
            {
                // No Type/Class that matches on the requested model to this WebAPI
                return completions;
            }

            // [PublishedModel(ContentTypeAlias="contact")]
            // Attempt to get the attribute on the type/class that we have requested
            // We can use the ContentTypeAlias to get descriptions for matching properties in the model/type/class
            var docTypeAttribute = modelType.GetCustomAttributes()
                .SingleOrDefault(x => x.GetType().Equals(typeof(PublishedModelAttribute))) as PublishedModelAttribute;

            // We can lookup/match the C# Model property (could be renamed in Model hence the attribute lookup above)
            // And match it up with the doctype alias in the C# custom attribute
            // to get the description in Umbraco
            var doctype = Services.ContentTypeService.Get(docTypeAttribute.ContentTypeAlias);
            var docTypeProps = doctype.CompositionPropertyTypes;

            // Properties on the ModelsBuilder class
            var modelProps = modelType.GetProperties();
            foreach (var modelProp in modelProps)
            {
                // string, IHtmlString, ...?
                var modelPropType = modelProp.PropertyType.Name;

                var docTypePropDesc = string.Empty;

                // Can't use a typeof and cast to it, as Umbraco.ModelsBuilder not referenced in Umbraco.Web
                // And would cause a circular reference
                // Hence the lovely/ugly code for fetching the attribute on its namespace and getting the value stored in the Alias property
                var modelPropAttr = modelProp.GetCustomAttributes()
                    .SingleOrDefault(x => x.GetType().FullName == "Umbraco.ModelsBuilder.Embedded.ImplementPropertyTypeAttribute");

                if(modelPropAttr != null)
                {
                    var modelPropAttrAliasVal = modelPropAttr.GetType().GetProperty("Alias").GetValue(modelPropAttr) as string;

                    // Go & find the matching doctype prop to get its description
                    var matchedDocTypeProp = docTypeProps.SingleOrDefault(x => x.Alias == modelPropAttrAliasVal);
                    if(matchedDocTypeProp != null)
                    {
                        docTypePropDesc = matchedDocTypeProp.Description;
                    }
                }                

                var completion = new CodeEditorCompletion
                {
                    Label = $"{modelProp.Name} <{modelPropType}>",
                    InsertText = modelProp.Name,
                    Documentation = docTypePropDesc
                };

                completions.Add(completion);
            }


            //{
            //    label: "BodyText <IHtmlString>",
            //    kind: monaco.languages.CompletionItemKind.Property,
            //    documentation: "The xxx to put you main content",
            //    insertText: "BodyText",
            //}

            return completions;
        }
    }

    public class CodeEditorCompletion
    {
        public string Label { get; set; }

        public string Documentation { get; set; }

        public string InsertText { get; set; }
    }
}
