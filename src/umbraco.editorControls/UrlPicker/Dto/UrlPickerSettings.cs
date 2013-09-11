using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.editorControls.UrlPicker.AjaxUpload;
using Umbraco.Core.IO;

namespace umbraco.editorControls.UrlPicker.Dto
{
    /// <summary>
    /// The settings DTO for the URL Picker - these are settings which won't change for the lifetime of the
    /// URL picker as a JavaScript object, and are required for it's working
    /// </summary>
    [Serializable]
    public class UrlPickerSettings
    {
        #region DTO Properties

        /// <summary>
        /// Which modes have been allowed for this picker
        /// </summary>
        public IEnumerable<UrlPickerMode> AllowedModes { get; set; }

        /// <summary>
        /// The mode which is initally selected
        /// </summary>
        public UrlPickerMode DefaultMode { get; set; }

        /// <summary>
        /// Store as comma seperated or XML
        /// </summary>
        public UrlPickerDataFormat DataFormat { get; set; }

        /// <summary>
        /// An integer unique to an instance of the UrlPickerDataEditor
        /// 
        /// Used (currently) to save files in the correct subfolder under the media folder
        /// </summary>
        public int? UniquePropertyId { get; set; }

        /// <summary>
        /// Whether the user can specify a title
        /// </summary>
        public bool EnableTitle { get; set; }

        /// <summary>
        /// Whether the user can specify the link to open in a new window
        /// </summary>
        public bool EnableNewWindow { get; set; }

        /// <summary>
        /// Whether the UrlPickerDataEditor is being used without the UrlPickerDataType
        /// (affects how the data editor is rendered).  Leave this as 'true' if you
        /// are using the data editor as a child of another data editor, or accessing
        /// it solely through the javascript API.
        /// </summary>
        public bool Standalone { get; set; }

        /// <summary>
        /// URL for an Ajax method which obtains a Content node's URL
        /// </summary>
        public string ContentNodeUrlMethod { get; set; }

        /// <summary>
        /// URL for an Ajax method which obtains a Media node's URL
        /// </summary>
        public string MediaNodeUrlMethod { get; set; }

        /// <summary>
        /// URL to AjaxUploadHandler.ashx
        /// </summary>
        public string AjaxUploadHandlerUrl { get; set; }

        /// <summary>
        /// GUID of the AjaxUploadHander
        /// </summary>
        public string AjaxUploadHandlerGuid { get; set; }

        #endregion

        /// <summary>
        /// Sets default settings
        /// </summary>
        public UrlPickerSettings()
        {
            // Add all modes
            AllowedModes = new List<UrlPickerMode>(Enum.GetValues(typeof(UrlPickerMode)).Cast<UrlPickerMode>());

            DataFormat = UrlPickerDataFormat.Xml;
            EnableTitle = true;
            EnableNewWindow = true;
            Standalone = true;
            
            // Add service method URLs
            var urlPickerServiceUrl = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + @"/plugins/UrlPicker/UrlPickerService.asmx/";

            ContentNodeUrlMethod = urlPickerServiceUrl + "ContentNodeUrl";
            MediaNodeUrlMethod = urlPickerServiceUrl + "MediaNodeUrl";

            // Add ajax upload handler URL & GUID
            AjaxUploadHandlerGuid = AjaxUploadHandler.Guid;
            AjaxUploadHandlerUrl = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + @"/plugins/UrlPicker/AjaxUploadHandler.ashx";
        }

        /// <summary>
        /// Validates a URL Picker state against the settings, to ensure there has been no trickery
        /// client-side
        /// </summary>
        /// <param name="state">The state DTO to validate against</param>
        /// <returns>Whether the state is valid</returns>
        public bool ValidateState(UrlPickerState state)
        {
            // Invalid if:
            // 1. If the mode is not allowed (or)
            // 2. The state fails its own validation
            if (!AllowedModes.Contains(state.Mode) || !state.Validates())
            {
                return false;
            }

            return true;
        }
    }
}
