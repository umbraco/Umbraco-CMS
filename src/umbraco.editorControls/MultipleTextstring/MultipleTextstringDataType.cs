using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace umbraco.editorControls.MultipleTextstring
{
    /// <summary>
    /// Data Editor for the Multiple Textstring data type.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MultipleTextstringDataType : AbstractDataEditor
    {
        /// <summary>
        /// The control for the Multiple Textstring data-editor.
        /// </summary>
        private MultipleTextstringControl m_Control = new MultipleTextstringControl();

        /// <summary>
        /// The Data object for the data-type.
        /// </summary>
        private IData m_Data;

        /// <summary>
        /// The PreValue Editor for the data-type.
        /// </summary>
        private IDataPrevalue m_PreValueEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTextstringDataType"/> class.
        /// </summary>
        public MultipleTextstringDataType()
        {
            // set the render control as the placeholder
            this.RenderControl = this.m_Control;

            // assign the initialise event for the control
            this.m_Control.Init += new EventHandler(this.m_Control_Init);

            // assign the save event for the data-type/editor
            this.DataEditorControl.OnSave += this.DataEditorControl_OnSave;
        }

        /// <summary>
        /// Gets the id of the data-type.
        /// </summary>
        /// <value>The id of the data-type.</value>
        public override Guid Id
        {
            get
            {
                return new Guid(DataTypeGuids.MultipleTextstringId);
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
                return "Multiple Textstring";
            }
        }

        /// <summary>
        /// Gets the data for the data-type.
        /// </summary>
        /// <value>The data for the data-type.</value>
        public override IData Data
        {
            get
            {
                if (this.m_Data == null)
                {
                    this.m_Data = new CsvToXmlData(this, "values", "value", new[] { Environment.NewLine });
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
                if (this.m_PreValueEditor == null)
                {
                    this.m_PreValueEditor = new MultipleTextstringPrevalueEditor(this);
                }

                return this.m_PreValueEditor;
            }
        }

        /// <summary>
        /// Handles the Init event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void m_Control_Init(object sender, EventArgs e)
        {
            var options =
                ((MultipleTextstringPrevalueEditor)this.PrevalueEditor).GetPreValueOptions<MultipleTextstringOptions>();

            if (options == null)
            {
                // load defaults
                options = new MultipleTextstringOptions(true);
            }

            // check if the data value is available...
            if (this.Data.Value != null)
            {
                // set the value of the control
                this.m_Control.Values = this.Data.Value.ToString();
            }

            // set the controls options
            this.m_Control.Options = options;
        }

        /// <summary>
        /// Saves the data for the editor control.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DataEditorControl_OnSave(EventArgs e)
        {
            this.Data.Value = this.m_Control.Values;
        }
    }
}
