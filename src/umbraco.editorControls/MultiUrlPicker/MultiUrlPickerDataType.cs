using System;
using umbraco.editorControls.MultiUrlPicker.Dto;
using umbraco.editorControls.UrlPicker;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using umbraco.editorControls;
using Umbraco.Core;

namespace umbraco.editorControls.MultiUrlPicker
{
    /// <summary>
    /// Description here:
    /// http://ucomponents.org/data-types/multi-url-picker/
    /// </summary>
    public class MultiUrlPickerDataType : AbstractDataEditor
    {
        private MultiUrlPickerDataEditor _dataEditor = new MultiUrlPickerDataEditor();

        private MultiUrlPickerPreValueEditor _preValueEditor;

        private IData _data;

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public override Guid Id
        {
            get
            {
                return new Guid(Constants.PropertyEditors.MultiUrlPicker);
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
                return "Multi-URL Picker";
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
                    _preValueEditor = new MultiUrlPickerPreValueEditor(this);

                return _preValueEditor;
            }
        }

        /// <summary>
        /// Get prevalue settings
        /// </summary>
        public MultiUrlPickerSettings Settings
        {
            get
            {
                return ((MultiUrlPickerPreValueEditor)PrevalueEditor).Settings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlPickerDataType"/> class.
        /// </summary>
        /// 
        public MultiUrlPickerDataType()
            : base()
        {
            RenderControl = _dataEditor;

            // Events
            _dataEditor.Init += new EventHandler(DataEditor_Init);
            _dataEditor.Load += new EventHandler(DataEditor_Load);
            DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        }

        /// <summary>
        /// Handles the Load event of the _dataEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditor_Load(object sender, EventArgs e)
        {
            if (!_dataEditor.Page.IsPostBack && !string.IsNullOrEmpty((string)this.Data.Value))
            {
                _dataEditor.State = MultiUrlPickerState.Deserialize((string)this.Data.Value);
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
            settings.UrlPickerSettings.UniquePropertyId = settings.UniquePropertyId;
            settings.Standalone = false;
            _dataEditor.Settings = settings;
        }

        /// <summary>
        /// Datas the editor control_ on save.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditorControl_OnSave(EventArgs e)
        {
            var state = _dataEditor.State;
            
            // Check the state validates
            this.Data.Value = (state == null || !Settings.ValidateState(state))
                ? null 
                : state.Serialize(Settings.DataFormat, false);
        }
    }
}