using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.Slider
{
    /// <summary>
    /// The jQuery UI Slider control.
    /// </summary>
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "ui/jquery.alphanumeric.js", "UmbracoClient")]
    [ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Css, "DateTimePicker/datetimepicker.css", "UmbracoClient")]
    [ValidationProperty("Text")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SliderControl : PlaceHolder
    {
        /// <summary>
        /// Gets or sets the slider options.
        /// </summary>
        /// <value>The slider options.</value>
        public SliderOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text value.</value>
        public string Text
        {
            get
            {
                return this.TextBoxControl.Text;
            }

            set
            {
                if (this.TextBoxControl == null)
                {
                    this.TextBoxControl = new TextBox();
                }

                this.TextBoxControl.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the TextBox control that contains the value(s) of the slider.
        /// </summary>
        /// <value>The text box control.</value>
        protected TextBox TextBoxControl { get; set; }

        /// <summary>
        /// Gets or sets the HtmlGenericControl control for the slider &lt;div&gt; tag.
        /// </summary>
        /// <value>The div slider control.</value>
        protected HtmlGenericControl DivSliderControl { get; set; }

        /// <summary>
        /// Initialize the control, make sure children are created
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnsureChildControls();
        }

        /// <summary>
        /// Add the resources (sytles/scripts)
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.EnsureChildControls();

            var divStyle = this.Options.Orientation.Equals("vertical") ? "float:left;margin:0 10px 0 0;height:150px;" : "float:left;margin:7px 10px 0 0;width:342px;";

            this.DivSliderControl = new HtmlGenericControl("div");
            this.DivSliderControl.ID = this.DivSliderControl.ClientID;
            this.DivSliderControl.Attributes.Add("style", divStyle);
            this.Controls.Add(this.DivSliderControl);

            this.TextBoxControl = new TextBox();
            this.TextBoxControl.ID = this.TextBoxControl.ClientID;
            this.TextBoxControl.CssClass = "guiInputTextTiny";
            this.TextBoxControl.Attributes.Add("style", "float:left;width:40px;");
            this.TextBoxControl.MaxLength = this.Options.EnableRange ? (this.Options.MaxValue.ToString().Length * 2) + 1 : this.Options.MaxValue.ToString().Length;
            this.Controls.Add(this.TextBoxControl);
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute("id", this.ClientID);
            writer.AddAttribute("class", "jqueryui-slider");
            writer.AddAttribute("style", "float:left;width:400px;");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            this.DivSliderControl.RenderControl(writer);

            this.TextBoxControl.RenderControl(writer);

            writer.RenderEndTag();

            // construct slider options
            bool hasMultipleValues = false;
            var options = new StringBuilder();

            // add range
            if (this.Options.EnableRange)
            {
                if (!string.IsNullOrEmpty(this.Options.RangeValue))
                {
                    options.Append("range: '").Append(this.Options.RangeValue).Append("', ");
                }
                else
                {
                    // the options can only have multiple values if the range is set to 'true'.
                    if (this.Options.Value2 >= this.Options.Value)
                    {
                        options.Append("range: true,values: [").Append(this.Options.Value).Append(',').Append(this.Options.Value2).Append("],");
                        hasMultipleValues = true;
                    }
                }
            }

            // add value - if multiple values have not been already set.
            if (!hasMultipleValues)
            {
                options.Append("value: ").Append(this.Options.Value).Append(',');
            }

            // add min.max values
            if (this.Options.MaxValue > this.Options.MinValue)
            {
                options.Append("min: ").Append(this.Options.MinValue).Append(',');
                options.Append("max: ").Append(this.Options.MaxValue).Append(',');
            }

            // add step increments
            if (this.Options.EnableStep)
            {
                options.Append("step: ").Append(this.Options.StepValue).Append(',');
            }

            // add orientation
            if (!string.IsNullOrEmpty(this.Options.Orientation))
            {
                options.Append("orientation: '").Append(this.Options.Orientation).Append("'").Append(',');
            }

            // add jquery window load event to create the js slider
            var javascriptMethod = string.Format(
                "jQuery('#{0}').slider({{ {2} slide: function(e, ui) {{ $('#{1}').val(ui.value{3}); }} }}); $('#{1}').val($('#{0}').slider('value{3}')); jQuery('#{1}').numeric({4});",
                this.DivSliderControl.ClientID,
                this.TextBoxControl.ClientID,
                options,
                (hasMultipleValues ? "s" : string.Empty),
                (hasMultipleValues ? "{ allow: ',' }" : string.Empty));
            var javascript = string.Concat("<script type='text/javascript'>jQuery(window).load(function(){", javascriptMethod, "});</script>");
            writer.WriteLine(javascript);

            if (this.Options.EnableRange || !string.IsNullOrEmpty(this.Options.RangeValue))
            {
                // add CSS to override the default style for '.ui-slider-range' (which is used for the DateTime Picker)
                var css = string.Format("<style type='text/css'>#{0} .ui-slider-range {{background-image: none;}}</style>", this.ClientID);
                writer.WriteLine(css);
            }
        }
    }
}