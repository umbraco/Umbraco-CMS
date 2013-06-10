using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MediaModelMapper
    {
        private readonly ApplicationContext _applicationContext;
        private readonly ProfileModelMapper _profileMapper;

        public MediaModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
        {
            _applicationContext = applicationContext;
            _profileMapper = profileMapper;
        }
       
        internal ContentItemBasic<ContentPropertyBasic> ToMediaItemSimple(IMedia media)
        {
            return CreateMedia<ContentItemBasic<ContentPropertyBasic>, ContentPropertyBasic>(media, null, null);
        } 

        /// <summary>
        /// Creates a new content item
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <typeparam name="TContentProperty"></typeparam>
        /// <param name="media"></param>
        /// <param name="contentCreatedCallback"></param>
        /// <param name="propertyCreatedCallback"></param>
        /// <param name="createProperties"></param>
        /// <returns></returns>
        private TContent CreateMedia<TContent, TContentProperty>(IMedia media,
            Action<TContent, IMedia> contentCreatedCallback = null,
            Action<TContentProperty, Property, PropertyEditor> propertyCreatedCallback = null, 
            bool createProperties = true)
            where TContent : ContentItemBasic<TContentProperty>, new()
            where TContentProperty : ContentPropertyBasic, new()
        {
            var result = new TContent
                {
                    Id = media.Id,
                    Owner = _profileMapper.ToBasicUser(media.GetCreatorProfile()),
                    Updator = null,
                    ParentId = media.ParentId,
                    UpdateDate = media.UpdateDate,
                    CreateDate = media.CreateDate,
                    ContentTypeAlias = media.ContentType.Alias
                };
            if (createProperties)
                result.Properties = media.Properties.Select(p => CreateProperty(p, propertyCreatedCallback)).ToArray();
            if (contentCreatedCallback != null)
                contentCreatedCallback(result, media);
            return result;
        }

        /// <summary>
        /// Creates the property with the basic property values mapped
        /// </summary>
        /// <typeparam name="TContentProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private static TContentProperty CreateProperty<TContentProperty>(
            Property property, 
            Action<TContentProperty, Property, PropertyEditor> callback = null)
            where TContentProperty : ContentPropertyBasic, new()
        {
            var editor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (editor == null)
            {
                //TODO: Remove this check as we shouldn't support this at all!
                var legacyEditor = DataTypesResolver.Current.GetById(property.PropertyType.DataTypeId);
                if (legacyEditor == null)
                {
                    throw new NullReferenceException("The property editor with id " + property.PropertyType.DataTypeId + " does not exist");   
                }

                var legacyResult = new TContentProperty
                {
                    Id = property.Id,
                    Value = property.Value.ToString()
                };
                if (callback != null) callback(legacyResult, property, null);
                return legacyResult;

            }
            var result = new TContentProperty
                {
                    Id = property.Id,
                    Value = editor.ValueEditor.SerializeValue(property.Value)
                };
            if (callback != null) callback(result, property, editor);
            return result;
        }
    }
}
