using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Composing;
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

                        Current.Logger.Warn(typeof(LegacyPropertyEditorIdToAliasConverter), "A legacy GUID id was generated for property editor " + alias + ". This occurs when the legacy APIs are used and done to attempt to maintain backwards compatibility. Consider upgrading all code to use the new Services APIs instead to avoid any potential issues.");

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
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.CheckBoxList), Constants.PropertyEditors.Aliases.CheckBoxList);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.ColorPicker), Constants.PropertyEditors.Aliases.ColorPicker);
            CreateMap(Guid.Parse(Constants.PropertyEditors.ContentPicker), Constants.PropertyEditors.ContentPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.Date), Constants.PropertyEditors.Aliases.Date);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.DateTime), Constants.PropertyEditors.Aliases.DateTime);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.DropDownList), Constants.PropertyEditors.Aliases.DropDownList);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.DropDownListMultiple), Constants.PropertyEditors.Aliases.DropDownListMultiple);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.DropdownlistMultiplePublishKeys), Constants.PropertyEditors.Aliases.DropdownlistMultiplePublishKeys);
            CreateMap(Guid.Parse(Constants.PropertyEditors.DropdownlistPublishingKeys), Constants.PropertyEditors.Aliases.DropdownlistPublishKeys);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.FolderBrowser), Constants.PropertyEditors.Aliases.FolderBrowser);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.Integer), Constants.PropertyEditors.Aliases.Integer);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.ListView), Constants.PropertyEditors.Aliases.ListView);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.MacroContainer), Constants.PropertyEditors.Aliases.MacroContainer);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MediaPicker), Constants.PropertyEditors.MediaPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MemberPicker), Constants.PropertyEditors.MemberPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.MultiNodeTreePicker), Constants.PropertyEditors.MultiNodeTreePickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.MultipleTextstring), Constants.PropertyEditors.Aliases.MultipleTextstring);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.NoEdit), Constants.PropertyEditors.Aliases.NoEdit);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.RadioButtonList), Constants.PropertyEditors.Aliases.RadioButtonList);
            CreateMap(Guid.Parse(Constants.PropertyEditors.RelatedLinks), Constants.PropertyEditors.RelatedLinksAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.Slider), Constants.PropertyEditors.Aliases.Slider);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.Tags), Constants.PropertyEditors.Aliases.Tags);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.Textbox), Constants.PropertyEditors.Aliases.Textbox);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.TextboxMultiple), Constants.PropertyEditors.Aliases.TextboxMultiple);
            CreateMap(Guid.Parse(Constants.PropertyEditors.TinyMCEv3), Constants.PropertyEditors.Aliases.TinyMce);
            CreateMap(Guid.Parse(Constants.PropertyEditors.TrueFalse), Constants.PropertyEditors.Aliases.Boolean);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.UserPicker), Constants.PropertyEditors.Aliases.UserPicker);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.UploadField), Constants.PropertyEditors.Aliases.UploadField);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.XPathCheckBoxList), Constants.PropertyEditors.Aliases.XPathCheckBoxList);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.XPathDropDownList), Constants.PropertyEditors.Aliases.XPathDropDownList);
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.ImageCropper), Constants.PropertyEditors.Aliases.ImageCropper);

            //Being mapped to different editors
            //TODO: Map this somewhere!
            CreateMap(Guid.Parse(Constants.PropertyEditors.Aliases.PickerRelations), Constants.PropertyEditors.Aliases.PickerRelations);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UltimatePicker), Constants.PropertyEditors.ContentPickerAlias);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UltraSimpleEditor), Constants.PropertyEditors.Aliases.MarkdownEditor);

            //Not being converted - convert to label
            CreateMap(Guid.Parse(Constants.PropertyEditors.DictionaryPicker), Constants.PropertyEditors.Aliases.NoEdit);
            CreateMap(Guid.Parse(Constants.PropertyEditors.UmbracoUserControlWrapper), Constants.PropertyEditors.Aliases.NoEdit);


        }

    }
}
