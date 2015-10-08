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

        public DataTypeBasic GetAssignedListViewDataType(int contentTypeId)
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
        /// Gets all user defined properties.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            return ApplicationContext.Services.ContentTypeService.GetAllPropertyTypeAliases();
        }

        public ContentPropertyDisplay GetPropertyTypeScaffold(int id)
        {
            var dataTypeDiff = Services.DataTypeService.GetDataTypeDefinitionById(id);

            if (dataTypeDiff == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var preVals = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(id);
            var editor = PropertyEditorResolver.Current.GetByAlias(dataTypeDiff.PropertyEditorAlias);

            return new ContentPropertyDisplay()
            {
                Editor = dataTypeDiff.PropertyEditorAlias,
                Validation = new PropertyTypeValidation() { },
                View = editor.ValueEditor.View,
                Config = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals)
            };
        }

        public dynamic GetSafeAlias(string value, bool camelCase = true)
        {
            var returnValue = (string.IsNullOrWhiteSpace(value)) ? string.Empty : value.ToSafeAlias(camelCase);
            dynamic returnObj = new System.Dynamic.ExpandoObject();
            returnObj.alias = returnValue;
            returnObj.original = value;
            returnObj.camelCase = camelCase;

            return returnObj;
        }



        public string TranslateItem(string text)
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