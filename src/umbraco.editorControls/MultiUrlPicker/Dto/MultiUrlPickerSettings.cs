using System;
using System.Linq;
using umbraco.editorControls.UrlPicker;
using umbraco.editorControls.UrlPicker.Dto;

namespace umbraco.editorControls.MultiUrlPicker.Dto
{
    /// <summary>
    /// The settings DTO for the Multi URL Picker
    /// </summary>
    [Serializable]
    public class MultiUrlPickerSettings
    {
        #region DTO Properties

        /// <summary>
        /// The format the state is to be stored in
        /// </summary>
        public UrlPickerDataFormat DataFormat { get; set; }

        /// <summary>
        /// The settings for the UrlPicker data editor
        /// </summary>
        public UrlPickerSettings UrlPickerSettings { get; set; }

        /// <summary>
        /// The default state for the UrlPicker, when a new instance is created
        /// </summary>
        public UrlPickerState UrlPickerDefaultState { get; set; }

        /// <summary>
        /// An integer unique to this instance of the MultiUrlPicker data editor
        /// </summary>
        public int? UniquePropertyId { get; set; }

        /// <summary>
        /// Whether the MultiUrlPickerDataEditor is being used without the MultiUrlPickerDataType
        /// (affects how the data editor is rendered).  Leave this as 'true' if you
        /// are using the data editor as a child of another data editor, or accessing
        /// it solely through the javascript API.
        /// </summary>
        public bool Standalone { get; set; }

        #endregion

        /// <summary>
        /// Sets defaults
        /// </summary>
        public MultiUrlPickerSettings()
        {
            UrlPickerSettings = new UrlPickerSettings();
            UrlPickerDefaultState = new UrlPickerState(UrlPickerSettings);
            Standalone = true;
        }

        /// <summary>
        /// Checks the state is completely valid
        /// </summary>
        /// <param name="state">State to validate</param>
        /// <returns>Validation success</returns>
        public bool ValidateState(MultiUrlPickerState state)
        {
            // Do any items fail validation, or miss a URL?
            return !state.Items.Any(x => !UrlPickerSettings.ValidateState(x));
        }
    }
}
