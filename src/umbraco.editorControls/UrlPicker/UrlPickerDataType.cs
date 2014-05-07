using System;
using umbraco.editorControls.UrlPicker.Dto;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using umbraco.editorControls;
using Umbraco.Core;

namespace umbraco.editorControls.UrlPicker
{
    /// <summary>
    /// Description here:
    /// http://ucomponents.org/data-types/url-picker/
    /// </summary>
    public class UrlPickerDataType : AbstractDataEditor
    {
        /// <summary>
        /// 
        /// </summary>
        private UrlPickerDataEditor _dataEditor;

        /// <summary>
        /// 
        /// </summary>
        private UrlPickerPreValueEditor _preValueEditor;

        /// <summary>
        /// 
        /// </summary>
        private IData _data;

        /// <summary>
        /// Gets the content editor.
        /// </summary>
        /// <value>The content editor.</value>
        public UrlPickerDataEditor ContentEditor
        {
            get
            {
                return _dataEditor ?? (_dataEditor = new UrlPickerDataEditor());
            }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public override Guid Id
        {
            get
            {
                return new Guid(Constants.PropertyEditors.UrlPicker);
            }
        }

        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
        public override string DataTypeName
        {
            get
            {
                return "URL Picker";
            }
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public override IData Data
        {
            get
            {
                if (this._data == null)
                {
                    if (Settings.DataFormat == UrlPickerDataFormat.Xml)
                    {
                        this._data = new XmlData(this);
                    }
                    else
                    {
                        this._data = new umbraco.cms.businesslogic.datatype.DefaultData(this);
                    }
                }

                return this._data;
            }
        }

        /// <summary>
        /// Gets the prevalue editor.
        /// </summary>
        /// <value>The prevalue editor.</value>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_preValueEditor == null)
                    _preValueEditor = new UrlPickerPreValueEditor(this);

                return _preValueEditor;
            }
        }

        /// <summary>
        /// Get prevalue settings
        /// </summary>
        public UrlPickerSettings Settings
        {
            get
            {
                return ((UrlPickerPreValueEditor)PrevalueEditor).Settings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlPickerDataType"/> class.
        /// </summary>
        public UrlPickerDataType()
        {
            RenderControl = this.ContentEditor;

            // Events
            this.ContentEditor.Init += new EventHandler(DataEditor_Init);
            this.ContentEditor.Load += new EventHandler(DataEditor_Load);
            DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        }

        /// <summary>
        /// Handles the Load event of the _dataEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditor_Load(object sender, EventArgs e)
        {
            if (!this.ContentEditor.Page.IsPostBack && !string.IsNullOrEmpty((string)this.Data.Value))
            {
                this.ContentEditor.State = UrlPickerState.Deserialize((string)this.Data.Value);
            }
        }

        /// <summary>
        /// Handles the Init event of the _dataEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditor_Init(object sender, EventArgs e)
        {
            // Fill DataEditor with the prevalue settings and a unique ID
            var settings = Settings;
            settings.UniquePropertyId = ((umbraco.cms.businesslogic.datatype.DefaultData)this.Data).PropertyId;
            settings.Standalone = false;
            this.ContentEditor.Settings = settings;
        }

        /// <summary>
        /// Datas the editor control_ on save.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditorControl_OnSave(EventArgs e)
        {
            var state = this.ContentEditor.State;

            // Must be a valid state
            this.Data.Value = (state == null || !Settings.ValidateState(state))
                ? null 
                : state.Serialize(Settings.DataFormat);
        }
    }
}