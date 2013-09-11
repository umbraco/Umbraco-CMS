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
    /// http://ucomponents.codeplex.com/wikipage?title=MultiUrlPicker
    /// </summary>
    public class MultiUrlPickerDataType : AbstractDataEditor
    {
        /// <summary>
        /// 
        /// </summary>
        private MultiUrlPickerDataEditor m_DataEditor = new MultiUrlPickerDataEditor();

        /// <summary>
        /// 
        /// </summary>
        private MultiUrlPickerPreValueEditor m_PreValueEditor;

        /// <summary>
        /// 
        /// </summary>
        private IData m_Data;

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
                if (this.m_Data == null)
                {
                    if (Settings.DataFormat == UrlPickerDataFormat.Xml)
                    {
                        this.m_Data = new XmlData(this);
                    }
                    else
                    {
                        this.m_Data = new umbraco.cms.businesslogic.datatype.DefaultData(this);
                    }
                }

                return this.m_Data;
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
                if (m_PreValueEditor == null)
                    m_PreValueEditor = new MultiUrlPickerPreValueEditor(this);
                return m_PreValueEditor;
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
            RenderControl = m_DataEditor;

            // Events
            m_DataEditor.Init += new EventHandler(m_DataEditor_Init);
            m_DataEditor.Load += new EventHandler(m_DataEditor_Load);
            DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        }

        /// <summary>
        /// Handles the Load event of the m_DataEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void m_DataEditor_Load(object sender, EventArgs e)
        {
            if (!m_DataEditor.Page.IsPostBack && !string.IsNullOrEmpty((string)this.Data.Value))
            {
                m_DataEditor.State = MultiUrlPickerState.Deserialize((string)this.Data.Value);
            }
        }

        /// <summary>
        /// Handles the Init event of the m_DataEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void m_DataEditor_Init(object sender, EventArgs e)
        {
            // Fill DataEditor with the prevalue settings and a unique ID
            var settings = Settings;
            settings.UniquePropertyId = ((umbraco.cms.businesslogic.datatype.DefaultData)this.Data).PropertyId;
            settings.UrlPickerSettings.UniquePropertyId = settings.UniquePropertyId;
            settings.Standalone = false;
            m_DataEditor.Settings = settings;
        }

        /// <summary>
        /// Datas the editor control_ on save.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataEditorControl_OnSave(EventArgs e)
        {
            var state = m_DataEditor.State;
            
            // Check the state validates
            this.Data.Value = (state == null || !Settings.ValidateState(state))
                ? null 
                : state.Serialize(Settings.DataFormat, false);
        }
    }
}