using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace umbraco.editorControls.Slider
{
    /// <summary>
    /// A jQuery UI Slider data-type for Umbraco.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SliderDataType : AbstractDataEditor
    {
        /// <summary>
        /// The SliderControl.
        /// </summary>
        private SliderControl m_Control = new SliderControl();

        /// <summary>
        /// The PreValue Editor for the data-type.
        /// </summary>
        private SliderPrevalueEditor m_PreValueEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderDataType"/> class.
        /// </summary>
        public SliderDataType()
            : base()
        {
            // set the render control as the placeholder
            this.RenderControl = this.m_Control;

            // assign the initialise event for the placeholder
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
                return new Guid(DataTypeGuids.SliderId);
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
                return "Slider";
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
                    this.m_PreValueEditor = new SliderPrevalueEditor(this);
                }

                return this.m_PreValueEditor;
            }
        }

        /// <summary>
        /// Handles the Init event of the m_Placeholder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void m_Control_Init(object sender, EventArgs e)
        {
            // get the slider options from the Prevalue Editor.
            var options = ((SliderPrevalueEditor)this.PrevalueEditor).GetPreValueOptions<SliderOptions>();

            // set the value of the control (not on PostBack)
            if (!this.m_Control.Page.IsPostBack && this.Data.Value != null)
            {
                var data = this.Data.Value.ToString();
                if (data.Length > 0)
                {
                    double value1, value2;
                    var values = data.Split(',');

                    if (double.TryParse(values[0], out value1))
                    {
                        options.Value = value1;

                        if (values.Length > 1 && double.TryParse(values[1], out value2))
                        {
                            options.Value2 = value2;
                        }
                    }
                }
            }

            // set the slider options
            this.m_Control.Options = options;
        }

        /// <summary>
        /// Saves the editor control value.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DataEditorControl_OnSave(EventArgs e)
        {
            // set the values (on PostBack)
            var value1 = this.m_Control.Options.MinValue;
            var value2 = this.m_Control.Options.MaxValue;
            var values = this.m_Control.Text.Split(',');

            if (double.TryParse(values[0], out value1))
            {
                this.m_Control.Options.Value = value1;

                if (values.Length > 1 && double.TryParse(values[1], out value2))
                {
                    this.m_Control.Options.Value2 = value2;
                }
            }

            // save the value of the control
            if (values.Length > 1 && value2 >= this.m_Control.Options.MinValue
                && value2 <= this.m_Control.Options.MaxValue)
            {
                this.Data.Value = string.Concat(value1, ',', value2);
            }
            else if (value1 >= this.m_Control.Options.MinValue && value1 <= this.m_Control.Options.MaxValue)
            {
                int value1int;

                // return an integer instead of double if applicable
                if (this.m_Control.Options.DBType == DBTypes.Integer && int.TryParse(value1.ToString(), out value1int))
                {
                    this.Data.Value = value1int;
                }
                else
                {
                    this.Data.Value = value1.ToString();
                }
            }
        }
    }
}