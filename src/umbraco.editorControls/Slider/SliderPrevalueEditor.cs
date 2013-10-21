using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.Slider
{
    /// <summary>
    /// The PreValue Editor for the Slider data-type.
    /// </summary>
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Css, "DateTimePicker/datetimepicker.css", "UmbracoClient")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SliderPrevalueEditor : AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// The DropDownList for the database data-type.
        /// </summary>
        private DropDownList DatabaseDataType;

        /// <summary>
        /// The CheckBox control to enable the range for the slider.
        /// </summary>
        private CheckBox EnableRange;

        /// <summary>
        /// The CheckBox control to enable incremental steps for the slider.
        /// </summary>
        private CheckBox EnableStep;

        /// <summary>
        /// The TextBox control for the minimum value of the slider.
        /// </summary>
        private TextBox MinValue;

        /// <summary>
        /// The TextBox control for the maximum value of the slider.
        /// </summary>
        private TextBox MaxValue;

        /// <summary>
        /// The DropDownList control for the orientation of the slider.
        /// </summary>
        private DropDownList Orientation;

        /// <summary>
        /// The DropDownList control for the range value.
        /// </summary>
        private DropDownList RangeValue;

        /// <summary>
        /// The TextBox control for the incremental step value.
        /// </summary>
        private TextBox StepValue;

        /// <summary>
        /// The TextBox control for the first value input.
        /// </summary>
        private TextBox Value;

        /// <summary>
        /// The TextBox control for the second value input.
        /// </summary>
        private TextBox Value2;

        ////private SliderControl PreviewSlider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderPrevalueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public SliderPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
            : base(dataType)
        {
        }

        /// <summary>
        /// Saves the data-type PreValue options.
        /// </summary>
        public override void Save()
        {
            // set the database data-type
            this.m_DataType.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), this.DatabaseDataType.SelectedValue);

            // parse the integers & doubles
            double maxValue, minValue, value, value2, stepValue;
            double.TryParse(this.MaxValue.Text, out maxValue);
            double.TryParse(this.MinValue.Text, out minValue);
            double.TryParse(this.Value.Text, out value);
            double.TryParse(this.Value2.Text, out value2);
            double.TryParse(this.StepValue.Text, out stepValue);

            // set the options
            var options = new SliderOptions()
            {
                DBType = (DBTypes)Enum.Parse(typeof(DBTypes), this.DatabaseDataType.SelectedValue),
                EnableRange = this.EnableRange.Checked,
                EnableStep = this.EnableStep.Checked,
                MaxValue = maxValue,
                MinValue = minValue,
                Orientation = this.Orientation.SelectedValue,
                RangeValue = this.RangeValue.SelectedValue,
                StepValue = stepValue,
                Value = value,
                Value2 = value2
            };

            // save the options as JSON
            this.SaveAsJson(options);

            // toggle the non-default fields
            this.ToggleFields();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnsureChildControls();
        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // set-up child controls
            this.DatabaseDataType = new DropDownList() { ID = "DatabaseDataType" };
            this.EnableRange = new CheckBox() { ID = "EnableRange" };
            this.EnableStep = new CheckBox() { ID = "EnableStep" };
            this.MinValue = new TextBox() { ID = "MinValue", CssClass = "guiInputText slider-numeric slider-decimal" };
            this.MaxValue = new TextBox() { ID = "MaxValue", CssClass = "guiInputText slider-numeric slider-decimal" };
            this.Orientation = new DropDownList() { ID = "Orientation" };
            this.RangeValue = new DropDownList() { ID = "RangeValue" };
            this.StepValue = new TextBox() { ID = "StepValue", CssClass = "guiInputText slider-numeric slider-decimal" };
            this.Value = new TextBox() { ID = "Value", CssClass = "guiInputText slider-numeric slider-decimal" };
            this.Value2 = new TextBox() { ID = "Value2", CssClass = "guiInputText slider-numeric slider-decimal" };
            ////this.PreviewSlider = new SliderControl() { ID = "PreviewSlider", Options = new SliderOptions(true) };

            // add the database data-type options
            this.DatabaseDataType.Items.Clear();
            this.DatabaseDataType.Items.Add(DBTypes.Integer.ToString());
            //this.DatabaseDataType.Items.Add(DBTypes.Ntext.ToString());
            this.DatabaseDataType.Items.Add(DBTypes.Nvarchar.ToString());

            // add range options
            this.RangeValue.Items.Clear();
            this.RangeValue.Items.Add(string.Empty);
            this.RangeValue.Items.Add(new ListItem("Minimum Value", "min"));
            this.RangeValue.Items.Add(new ListItem("Maximum Value", "max"));

            // add orientation options
            this.Orientation.Items.Clear();
            this.Orientation.Items.Add(new ListItem("Horizontal (default)", "horizontal"));
            this.Orientation.Items.Add(new ListItem("Vertical", "vertical"));

            // add the child controls
            this.Controls.AddPrevalueControls(this.DatabaseDataType, this.EnableRange, this.EnableStep, this.MaxValue, this.MinValue, this.Orientation, this.RangeValue, this.StepValue, this.Value, this.Value2);
            ////this.Controls.Add(this.PreviewSlider);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // get PreValues, load them into the controls.
            var options = this.GetPreValueOptions<SliderOptions>();

            // if the options are null, then load the defaults
            if (options == null)
            {
                options = new SliderOptions(true);
            }

            // set the values
            this.DatabaseDataType.SelectedValue = this.m_DataType.DBType.ToString();
            this.EnableRange.Checked = options.EnableRange;
            this.EnableStep.Checked = options.EnableStep;
            this.MinValue.Text = options.MinValue.ToString();
            this.MaxValue.Text = options.MaxValue.ToString();
            this.Orientation.SelectedValue = options.Orientation;
            this.RangeValue.SelectedValue = options.RangeValue;
            this.StepValue.Text = options.StepValue.ToString();
            this.Value.Text = options.Value.ToString();
            this.Value2.Text = options.Value2.ToString();

            // toggle the non-default fields
            this.ToggleFields();
        }

        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // add property fields
            writer.AddPrevalueRow("Database Type:", this.DatabaseDataType);
            writer.AddPrevalueRow("Enable Range:", this.EnableRange);
            writer.AddPrevalueRow("Range:", this.RangeValue);
            writer.AddPrevalueRow("Initial Value:", this.Value);
            writer.AddPrevalueRow("Initial Value 2:", this.Value2);
            writer.AddPrevalueRow("Minimum Value:", this.MinValue);
            writer.AddPrevalueRow("Maximum Value:", this.MaxValue);
            writer.AddPrevalueRow("Enable Step Increments:", this.EnableStep);
            writer.AddPrevalueRow("Step Increments:", this.StepValue);
            writer.AddPrevalueRow("Orientation:", this.Orientation);

            ////writer.AddPrevalueRow("&nbsp;", new LiteralControl("<h2 class='propertypaneTitel'>Preview:</h2><br/>"), this.PreviewSlider);

            // add jquery window load event for toggling fields.
            var javascriptMethod = string.Format(
                @"
$('#{0}').click(function(){{
    var disable = !$(this).attr('checked');
    $('#{1},#{3}').attr('disabled', disable);
    $('#{6}').val(disable && !checkDecimals() ? 'Integer' : 'Nvarchar');
    if(!disable) disable = $('#{1}').val() != '';
    
}});
$('#{1}').change(function(){{
    var disable = $(this).val() != '';
    $('#{3}').attr('disabled', disable);
}});
$('#{4}').click(function(){{
    var disable = !$(this).attr('checked');
    $('#{5}').attr('disabled', disable);
}});
$('#{6}').change(function(){{
    var disable = $(this).val() == 'Integer';
    if (checkDecimals() && disable) {{
        $('#{6}').val('Nvarchar');
        alert('Please remove decimal points below if you wish to use the Integer datatype');  
    }}
    else {{
    $('#{0}').removeAttr('checked');
    $('#{1},#{3}').attr('disabled', disable);
    }}
}});
$('.slider-numeric').keydown(function(event) {{
    // Allow only backspace and delete
    if ( event.keyCode == 46 || event.keyCode == 8 || ($(this).hasClass('slider-decimal') && (event.keyCode == 110 || event.keyCode == 190))) {{
        // let it happen, don't do anything
    }} else {{
        // Ensure that it is a number and stop the keypress
        if ( (event.keyCode < 48 || event.keyCode > 57 ) && (event.keyCode < 96 || event.keyCode > 105 ) ) {{
            event.preventDefault();
        }}
    }}
}});
$('.slider-numeric').keyup(function(event) {{
    if ($('#{6}').val() != 'Nvarchar' && checkDecimals()) {{
        $('#{6}').val('Nvarchar');
    }}
}});
function checkDecimals() {{
    foundDecimals = false;
    $('.slider-numeric').each(function() {{
            if ($(this).val().indexOf('.') >= 0) {{
                foundDecimals = true;
                return false;
            }}  
        }});
    return foundDecimals;
}}
",
                this.EnableRange.ClientID,
                this.RangeValue.ClientID,
                this.Value.ClientID,
                this.Value2.ClientID,
                this.EnableStep.ClientID,
                this.StepValue.ClientID,
                this.DatabaseDataType.ClientID);
            var javascript = string.Concat("<script type='text/javascript'>jQuery(window).load(function(){", javascriptMethod, "});</script>");
            writer.WriteLine(javascript);
        }

        /// <summary>
        /// Toggles the fields.
        /// </summary>
        private void ToggleFields()
        {
            if (this.DatabaseDataType.SelectedIndex == 0)
            {
                this.EnableRange.Checked = false;
            }

            this.RangeValue.Enabled = this.EnableRange.Checked;
            this.Value2.Enabled = this.EnableRange.Checked && this.RangeValue.SelectedValue == string.Empty;
            this.StepValue.Enabled = this.EnableStep.Checked;
        }
    }
}
