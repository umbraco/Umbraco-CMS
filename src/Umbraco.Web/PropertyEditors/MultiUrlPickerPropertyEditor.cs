using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultiUrlPickerAlias, "Multi Url Picker", PropertyEditorValueTypes.Json, "multiurlpicker", Group = "pickers", Icon = "icon-link")]
    public class MultiUrlPickerPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultiUrlPickerPreValueEditor();
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new MultiUrlPickerPropertyValueEditor(base.CreateValueEditor());
        }

        private class MultiUrlPickerPreValueEditor : PreValueEditor
        {
            public MultiUrlPickerPreValueEditor()
            {
                Fields.Add(new PreValueField()
                {
                    Key = "ignoreUserStartNodes",
                    View = "boolean",
                    Name = "Ignore user start nodes",
                    Description = "Selecting this option allows a user to choose nodes that they normally don't have access to."
                });
                Fields.Add(new PreValueField
                {
                    Key = "minNumber",
                    View = "number",
                    Name = "Minimum number of items"
                });
                Fields.Add(new PreValueField
                {
                    Key = "maxNumber",
                    View = "number",
                    Name = "Maximum number of items"
                });
            }
        }

        private class MultiUrlPickerPropertyValueEditor : PropertyValueEditorWrapper
        {
            public MultiUrlPickerPropertyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
            {
            }

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null)
                    return Enumerable.Empty<object>();

                var value = property.Value.ToString();

                if (string.IsNullOrEmpty(value))
                    return Enumerable.Empty<object>();

                try
                {
                    var umbHelper = new UmbracoHelper(UmbracoContext.Current);
                    var services = ApplicationContext.Current.Services;
                    var entityService = services.EntityService;
                    var contentTypeService = services.ContentTypeService;
                    string deletedLocalization = null;
                    string recycleBinLocalization = null;

                    var dtos = JsonConvert.DeserializeObject<List<LinkDto>>(value);

                    var documentLinks = dtos.FindAll(link =>
                        link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Document
                    );

                    var mediaLinks = dtos.FindAll(link =>
                        link.Udi != null && link.Udi.EntityType == Constants.UdiEntityType.Media
                    );

                    var entities = new List<IUmbracoEntity>();
                    if (documentLinks.Count > 0)
                    {
                        entities.AddRange(
                            entityService.GetAll(UmbracoObjectTypes.Document,
                                documentLinks.Select(link => link.Udi.Guid).ToArray())
                        );
                    }

                    if (mediaLinks.Count > 0)
                    {
                        entities.AddRange(
                            entityService.GetAll(UmbracoObjectTypes.Media,
                                mediaLinks.Select(link => link.Udi.Guid).ToArray())
                        );
                    }

                    var links = new List<LinkDisplay>();
                    foreach (var dto in dtos)
                    {
                        var link = new LinkDisplay
                        {
                            Icon = "icon-link",
                            IsMedia = false,
                            Name = dto.Name,
                            Published = true,
                            QueryString = dto.QueryString,
                            Target = dto.Target,
                            Trashed = false,
                            Udi = dto.Udi,
                            Url = dto.Url ?? "",
                        };

                        links.Add(link);

                        if (dto.Udi == null)
                            continue;

                        var entity = entities.Find(e => e.Key == dto.Udi.Guid);
                        if (entity == null)
                        {
                            if (deletedLocalization == null)
                                deletedLocalization = services.TextService.Localize("general/deleted");

                            link.Published = false;
                            link.Trashed = true;
                            link.Url = deletedLocalization;
                        }
                        else
                        {
                            var entityType =
                                Equals(entity.AdditionalData["NodeObjectTypeId"], Constants.ObjectTypes.MediaGuid)
                                    ? Constants.UdiEntityType.Media
                                    : Constants.UdiEntityType.Document;

                            var udi = new GuidUdi(entityType, entity.Key);

                            var contentTypeAlias = (string)entity.AdditionalData["ContentTypeAlias"];
                            if (entity.Trashed)
                            {
                                if (recycleBinLocalization == null)
                                    recycleBinLocalization = services.TextService.Localize("general/recycleBin");

                                link.Trashed = true;
                                link.Url = recycleBinLocalization;
                            }

                            if (udi.EntityType == Constants.UdiEntityType.Document)
                            {
                                var contentType = contentTypeService.GetContentType(contentTypeAlias);

                                if (contentType == null)
                                    continue;

                                link.Icon = contentType.Icon;
                                link.Published = Equals(entity.AdditionalData["IsPublished"], true);

                                if (link.Trashed == false)
                                    link.Url = umbHelper.Url(entity.Id, UrlProviderMode.Relative);
                            }
                            else
                            {
                                link.IsMedia = true;

                                var mediaType = contentTypeService.GetMediaType(contentTypeAlias);

                                if (mediaType == null)
                                    continue;

                                link.Icon = mediaType.Icon;

                                if (link.Trashed)
                                    continue;

                                var media = umbHelper.TypedMedia(entity.Id);
                                if (media != null)
                                    link.Url = media.Url;
                            }
                        }
                    }
                    return links;
                }
                catch (Exception ex)
                {
                    ApplicationContext.Current.ProfilingLogger.Logger.Error<MultiUrlPickerPropertyValueEditor>($"Error getting links.\r\n{property.Value}", ex);
                }

                return base.ConvertDbToEditor(property, propertyType, dataTypeService);
            }

            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var value = editorValue.Value.ToString();

                if (string.IsNullOrEmpty(value))
                    return null;

                try
                {
                    return JsonConvert.SerializeObject(
                        from link in JsonConvert.DeserializeObject<List<LinkDisplay>>(value)
                        select new LinkDto
                        {
                            Name = link.Name,
                            QueryString = link.QueryString,
                            Target = link.Target,
                            Udi = link.Udi,
                            Url = link.Udi == null ? link.Url : null, // only save the url for external links
                        },
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                }
                catch (Exception ex)
                {
                    ApplicationContext.Current.ProfilingLogger.Logger.Error<MultiUrlPickerPropertyValueEditor>($"Error saving links.\r\n{editorValue.Value}", ex);
                }
                return base.ConvertEditorToDb(editorValue, currentValue);
            }
        }

        [DataContract]
        internal class LinkDto
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "queryString")]
            public string QueryString { get; set; }

            [DataMember(Name = "target")]
            public string Target { get; set; }

            [DataMember(Name = "udi")]
            public GuidUdi Udi { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }
        }
    }
}
