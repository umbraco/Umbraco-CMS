using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]    
    [PrefixlessBodyModelValidator]
    public abstract class ContentTypeControllerBase : UmbracoAuthorizedJsonController
    {
        private ICultureDictionary _cultureDictionary;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ContentTypeControllerBase()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected ContentTypeControllerBase(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        protected internal DataTypeBasic GetAssignedListViewDataType(int contentTypeId)
        {
            var objectType = Services.EntityService.GetObjectType(contentTypeId);

            switch (objectType)
            {
                case UmbracoObjectTypes.MemberType:     
                    var memberType = Services.MemberTypeService.Get(contentTypeId);
                    var dtMember = Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + memberType.Alias);
                    return dtMember == null
                        ? Mapper.Map<IDataTypeDefinition, DataTypeBasic>(
                            Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + "Member"))
                        : Mapper.Map<IDataTypeDefinition, DataTypeBasic>(dtMember);
                case UmbracoObjectTypes.MediaType:                
                    var mediaType = Services.ContentTypeService.GetMediaType(contentTypeId);
                    var dtMedia = Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + mediaType.Alias);
                    return dtMedia == null
                        ? Mapper.Map<IDataTypeDefinition, DataTypeBasic>(
                            Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + "Media"))
                        : Mapper.Map<IDataTypeDefinition, DataTypeBasic>(dtMedia);
                case UmbracoObjectTypes.DocumentType:
                    var docType = Services.ContentTypeService.GetContentType(contentTypeId);
                    var dtDoc = Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + docType.Alias);
                    return dtDoc == null
                        ? Mapper.Map<IDataTypeDefinition, DataTypeBasic>(
                            Services.DataTypeService.GetDataTypeDefinitionByName(Constants.Conventions.DataTypes.ListViewPrefix + "Content"))
                        : Mapper.Map<IDataTypeDefinition, DataTypeBasic>(dtDoc);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Validates the composition and adds errors to the model state if any are found then throws an error response if there are errors
        /// </summary>
        /// <param name="contentTypeSave"></param>
        /// <param name="composition"></param>
        /// <returns></returns>
        protected void ValidateComposition(ContentTypeSave contentTypeSave, IContentTypeComposition composition)
        {
            var validateAttempt = Services.ContentTypeService.ValidateComposition(composition);
            if (validateAttempt == false)
            {
                //if it's not successful then we need to return some model state for the property aliases that 
                // are duplicated
                var propertyAliases = validateAttempt.Result.Distinct();
                foreach (var propertyAlias in propertyAliases)
                {
                    //find the property relating to these
                    var prop = contentTypeSave.Groups.SelectMany(x => x.Properties).Single(x => x.Alias == propertyAlias);
                    var group = contentTypeSave.Groups.Single(x => x.Properties.Contains(prop));
                    var propIndex = group.Properties.IndexOf(prop);
                    var groupIndex = contentTypeSave.Groups.IndexOf(group);

                    var key = string.Format("Groups[{0}].Properties[{1}].Alias", groupIndex, propIndex);
                    ModelState.AddModelError(key, "Duplicate property aliases not allowed between compositions");
                }

                var display = Mapper.Map<ContentTypeDisplay>(composition);
                //map the 'save' data on top
                display = Mapper.Map(contentTypeSave, display);
                display.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
            }

        }

        protected string TranslateItem(string text)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return CultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        protected TContentType PerformPostSave<TContentType, TContentTypeDisplay>(
            ContentTypeSave contentTypeSave,
            Func<int, TContentType> getContentType,
            Action<TContentType> saveContentType,
            bool validateComposition = true,
            Action<ContentTypeSave> beforeCreateNew = null)
            where TContentType : IContentTypeComposition
            where TContentTypeDisplay : ContentTypeCompositionDisplay
        {
            var ctId = Convert.ToInt32(contentTypeSave.Id);
            
            if (ModelState.IsValid == false)
            {
                var ct = getContentType(ctId);
                //Required data is invalid so we cannot continue
                var forDisplay = Mapper.Map<TContentTypeDisplay>(ct);
                //map the 'save' data on top
                forDisplay = Mapper.Map(contentTypeSave, forDisplay);
                forDisplay.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
            }

            //filter out empty properties
            contentTypeSave.Groups = contentTypeSave.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentTypeSave.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }
            
            if (ctId > 0)
            {
                //its an update to an existing
                var found = getContentType(ctId);
                if (found == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                Mapper.Map(contentTypeSave, found);

                if (validateComposition)
                {
                    //NOTE: this throws an error response if it is not valid
                    ValidateComposition(contentTypeSave, found);
                }

                saveContentType(found);

                return found;
            }
            else
            {
                if (beforeCreateNew != null)
                {
                    beforeCreateNew(contentTypeSave);
                }

                //set id to null to ensure its handled as a new type
                contentTypeSave.Id = null;
                contentTypeSave.CreateDate = DateTime.Now;
                contentTypeSave.UpdateDate = DateTime.Now;

                //check if the type is trying to allow type 0 below itself - id zero refers to the currently unsaved type
                //always filter these 0 types out
                var allowItselfAsChild = false;
                if (contentTypeSave.AllowedContentTypes != null)
                {
                    allowItselfAsChild = contentTypeSave.AllowedContentTypes.Any(x => x == 0);
                    contentTypeSave.AllowedContentTypes = contentTypeSave.AllowedContentTypes.Where(x => x > 0).ToList();
                }

                //save as new
                var newCt = Mapper.Map<TContentType>(contentTypeSave);

                if (validateComposition)
                {
                    //NOTE: this throws an error response if it is not valid
                    ValidateComposition(contentTypeSave, newCt);
                }

                saveContentType(newCt);

                //we need to save it twice to allow itself under itself.
                if (allowItselfAsChild)
                {
                    //NOTE: This will throw if the composition isn't right... but it shouldn't be at this stage
                    newCt.AddContentType(newCt);
                    saveContentType(newCt);
                }
                return newCt;
            }
        }

        private ICultureDictionary CultureDictionary
        {
            get
            {
                return
                    _cultureDictionary ??
                    (_cultureDictionary = CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary());
            }
        }
        
    }
}