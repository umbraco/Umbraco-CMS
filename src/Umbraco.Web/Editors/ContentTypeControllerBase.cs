using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]    
    public abstract class ContentTypeControllerBase : UmbracoAuthorizedJsonController
    {
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
    
    }
}