using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.MultipleTextstring
{
    /// <summary>
    /// The PreValue Editor for the Multiple Textstring data-type.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MultipleTextstringPrevalueEditor : AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// The TextBox control for the maximum value of the control.
        /// </summary>
        private TextBox TextBoxMaximum;

        /// <summary>
        /// The TextBox control for the minimum value of the control.
        /// </summary>
        private TextBox TextBoxMinimum;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTextstringPrevalueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public MultipleTextstringPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
            : base(dataType, umbraco.cms.businesslogic.datatype.DBTypes.Ntext)
        {
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public override void Save()
        {
            // set the options
            var options = new MultipleTextstringOptions(true);

            // parse the maximum
            int maximum;
            if (int.TryParse(this.TextBoxMaximum.Text, out maximum))
            {
                if (maximum == 0)
                {
                    maximum = -1;
                }

                options.Maximum = maximum;
            }

            // parse the minimum
            int minimum;
            if (int.TryParse(this.TextBoxMinimum.Text, out minimum))
            {
                if (minimum == 0)
                {
                    minimum = -1;
                }

                options.Minimum = minimum;
            }

            // save the options as JSON
            this.SaveAsJson(options);
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // set-up child controls
            this.TextBoxMaximum = new TextBox() { ID = "Maximum", CssClass = "guiInputText" };
            this.TextBoxMinimum = new TextBox() { ID = "Minimum", CssClass = "guiInputText" };

            // add the child controls
            this.Controls.AddPrevalueControls(this.TextBoxMaximum, this.TextBoxMinimum);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // get PreValues, load them into the controls.
            var options = this.GetPreValueOptions<MultipleTextstringOptions>();

            // no options? use the default ones.
            if (options == null)
            {
                options = new MultipleTextstringOptions(true);
            }

            // set the values
            this.TextBoxMaximum.Text = options.Maximum.ToString();
            this.TextBoxMinimum.Text = options.Minimum.ToString();
        }

        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // add property fields
            writer.AddPrevalueRow("Minimum:", "Minimum number of rows to display.", this.TextBoxMinimum);
            writer.AddPrevalueRow("Maximum:", "Maximum number of rows to display.", this.TextBoxMaximum);
        }
    }
}
