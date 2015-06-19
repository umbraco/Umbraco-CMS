using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// This is used to map old legacy property editor GUID's to the new Property Editor alias (string) format.
    /// </summary>
    /// <remarks>
    /// This can be used by developers on application startup to register a mapping from their old ids to their new aliases and vice-versa.
    /// </remarks>
    public static class LegacyPropertyEditorIdToAliasConverter
    {

        public enum NotFoundLegacyIdResponseBehavior
        {
            ThrowException,
            ReturnNull,
            GenerateId
        }

        /// <summary>
        /// The map consists of a key which is always the GUID (lowercase, no hyphens + alias (trimmed))
        /// </summary>
        private static ConcurrentDictionary<string, Tuple<Guid, string>> _map = new ConcurrentDictionary<string, Tuple<Guid, string>>(); 

        /// <summary>
        /// Creates a map for the specified legacy id and property editor alias
        /// </summary>
        /// <param name="legacyId"></param>
        /// <param name="alias"></param>
        /// <returns>true if the map was created or false if it was already created</returns>
        public static bool CreateMap(Guid legacyId, string alias)
        {
            var key = legacyId.ToString("N").ToLowerInvariant() + alias.Trim();
            return _map.TryAdd(key, new Tuple<Guid, string>(legacyId, alias));
        }

        /// <summary>
        /// Gets an alias based on the legacy ID
        /// </summary>
        /// <param name="legacyId"></param>
        /// <param name="throwIfNotFound">if set to true will throw an exception if the map isn't found</param>
        /// <returns>Returns the alias if found otherwise null if not found</returns>
        public static string GetAliasFromLegacyId(Guid legacyId, bool throwIfNotFound = false)
        {
            var found = _map.FirstOrDefault(x => x.Value.Item1 == legacyId);
            if (found.Equals(default(KeyValuePair<string, Tuple<Guid, string>>)))
            {
                if (throwIfNotFound)
                {
                    throw new ObjectNotFoundException("Could not find a map for a property editor with a legacy id of " + legacyId + ". Consider using the new business logic APIs instead of the old obsoleted ones.");
                }
                return null;
            }
            return found.Value.Item2;
        }

        /// <summary>
        /// Gets a legacy Id based on the alias
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="notFoundBehavior"></param>
        /// <returns>Returns the legacy GUID of a property editor if found, otherwise returns null</returns>
        public static Guid? GetLegacyIdFromAlias(string alias, NotFoundLegacyIdResponseBehavior notFoundBehavior)
        {
            var found = _map.FirstOrDefault(x => x.Value.Item2 == alias);
            if (found.Equals(default(KeyValuePair<string, Tuple<Guid, string>>)))
            {
                switch (notFoundBehavior)
                {
                    case NotFoundLegacyIdResponseBehavior.ThrowException:
                        throw new ObjectNotFoundException("Could not find a map for a property editor with an alias of " + alias + ". Consider using the new business logic APIs instead of the old obsoleted ones.");
                    case NotFoundLegacyIdResponseBehavior.ReturnNull:
                        return null;
                    case NotFoundLegacyIdResponseBehavior.GenerateId:
                        var generated = alias.EncodeAsGuid();
                        CreateMap(generated, alias);

                        LogHelper.Warn(typeof(LegacyPropertyEditorIdToAliasConverter), "A legacy GUID id was generated for property editor " + alias + ". This occurs when the legacy APIs are used and done to attempt to maintain backwards compatibility. Consider upgrading all code to use the new Services APIs instead to avoid any potential issues.");

                        return generated;
                }
            }
            return found.Value.Item1;
        }
        
        internal static int Count()
        {
            return _map.Count;
        }

        internal static void Reset()
        {
            _map = new ConcurrentDictionary<string, Tuple<Guid, string>>();
        }

        /// <summary>
        /// A method that should be called on startup to register the mappings for the internal core editors
        /// </summary>
        internal static void CreateMappingsForCoreEditors()
        {
            CreateMap(Guid.Parse(Constants.PropertyEditors.CheckBoxList), Constants.PropertyEditors.CheckBoxListAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.ColorPicker), Constants.PropertyEditors.ColorPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.ContentPicker), Constants.PropertyEditors.ContentPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Date), Constants.PropertyEditors.DateAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.DateTime), Constants.PropertyEditors.DateTimeAlias);            
            CreateMap(Guid.Parse(Constants.PropertyEditors.DropDownList), Constants.PropertyEditors.DropDownListAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.DropDownListMultiple), Constants.PropertyEditors.DropDownListMultipleAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.DropdownlistMultiplePublishKeys), Constants.PropertyEditors.DropdownlistMultiplePublishKeysAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.DropdownlistPublishingKeys), Constants.PropertyEditors.DropdownlistPublishingKeysAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.FolderBrowser), Constants.PropertyEditors.FolderBrowserAlias);            
            CreateMap(Guid.Parse(Constants.PropertyEditors.Integer), Constants.PropertyEditors.IntegerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.ListView), Constants.PropertyEditors.ListViewAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MacroContainer), Constants.PropertyEditors.MacroContainerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MediaPicker), Constants.PropertyEditors.MediaPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MemberPicker), Constants.PropertyEditors.MemberPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MultiNodeTreePicker), Constants.PropertyEditors.MultiNodeTreePickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MultipleTextstring), Constants.PropertyEditors.MultipleTextstringAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.NoEdit), Constants.PropertyEditors.NoEditAlias);            
            CreateMap(Guid.Parse(Constants.PropertyEditors.RadioButtonList), Constants.PropertyEditors.RadioButtonListAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.RelatedLinks), Constants.PropertyEditors.RelatedLinksAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Slider), Constants.PropertyEditors.SliderAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Tags), Constants.PropertyEditors.TagsAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Textbox), Constants.PropertyEditors.TextboxAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.TextboxMultiple), Constants.PropertyEditors.TextboxMultipleAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.TinyMCEv3), Constants.PropertyEditors.TinyMCEAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.TrueFalse), Constants.PropertyEditors.TrueFalseAlias);            
            CreateMap(Guid.Parse(Constants.PropertyEditors.UserPicker), Constants.PropertyEditors.UserPickerAlias);            
            CreateMap(Guid.Parse(Constants.PropertyEditors.UploadField), Constants.PropertyEditors.UploadFieldAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.XPathCheckBoxList), Constants.PropertyEditors.XPathCheckBoxListAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.XPathDropDownList), Constants.PropertyEditors.XPathDropDownListAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.ImageCropper), Constants.PropertyEditors.ImageCropperAlias);

            //Being mapped to different editors
            //TODO: Map this somewhere!
            CreateMap(Guid.Parse(Constants.PropertyEditors.PickerRelations), Constants.PropertyEditors.PickerRelationsAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UltimatePicker), Constants.PropertyEditors.ContentPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UltraSimpleEditor), Constants.PropertyEditors.MarkdownEditorAlias);            

            //Not being converted - convert to label
            CreateMap(Guid.Parse(Constants.PropertyEditors.DictionaryPicker), Constants.PropertyEditors.NoEditAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UmbracoUserControlWrapper), Constants.PropertyEditors.NoEditAlias);
            
            
        }

    }
}
