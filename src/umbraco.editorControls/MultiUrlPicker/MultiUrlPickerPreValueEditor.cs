using System;
using System.Web.UI;
using umbraco.editorControls.MultiUrlPicker.Dto;
using umbraco.editorControls.UrlPicker;
using umbraco.editorControls.UrlPicker.Dto;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.MultiUrlPicker
{
    /// <summary>
    /// The MultiPreValueEditor for the MultiUrlPicker.
    /// </summary>
    public class MultiUrlPickerPreValueEditor : UrlPickerPreValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiUrlPickerPreValueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public MultiUrlPickerPreValueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
            : base(dataType)
        {
        }

        /// <summary>
        /// Retrieves the prevalues
        /// </summary>
        new public MultiUrlPickerSettings Settings
        {
            get
            {
                var urlPickerSettings = base.Settings;

                return new MultiUrlPickerSettings {
                    DataFormat = urlPickerSettings.DataFormat, // inherit data format for now
                    UrlPickerSettings = urlPickerSettings,
                    UrlPickerDefaultState = new UrlPickerState(urlPickerSettings)
                };
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        /// <summary>
        /// Prepopulates the controls with prevalue data
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        #region IDataPrevalue Members

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public override void Save()
        {
            base.Save();

            // Extra saving goes on here
        }

        #endregion
    }
}