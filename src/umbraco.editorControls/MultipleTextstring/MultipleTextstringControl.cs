using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco;
using umbraco.cms.businesslogic.datatype;

[assembly: WebResource("umbraco.editorControls.MultipleTextstring.MultipleTextstring.css", "text/css")]
[assembly: WebResource("umbraco.editorControls.MultipleTextstring.MultipleTextstring.js", "application/x-javascript")]

namespace umbraco.editorControls.MultipleTextstring
{
    /// <summary>
    /// The MultipleTextstring control sets a character limit on a TextBox.
    /// </summary>
    [ValidationProperty("IsValid")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MultipleTextstringControl : PlaceHolder
    {
        /// <summary>
        /// Field for the list of values.
        /// </summary>
        private List<string> values;

        /// <summary>
        /// The HiddenField to store the selected values.
        /// </summary>
        private HiddenField SelectedValues = new HiddenField();

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public MultipleTextstringOptions Options { get; set; }

        /// <summary>
        /// Gets the value of IsValid.
        /// </summary>
        /// <value>Returns 'Valid' if valid, otherwise an empty string.</value>
        public string IsValid
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Values))
                {
                    return "Valid";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public string Values
        {
            get
            {
                return this.SelectedValues.Value;
            }

            set
            {
                this.SelectedValues.Value = value;
            }
        }

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

            // Adds the client dependencies.
            this.RegisterEmbeddedClientResource("umbraco.editorControls.MultipleTextstring.MultipleTextstring.css", ClientDependencyType.Css);
            this.RegisterEmbeddedClientResource("umbraco.editorControls.MultipleTextstring.MultipleTextstring.js", ClientDependencyType.Javascript);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // initalise the string array/list.
            this.values = new List<string>();

            // load the values into a string array/list.
            if (!string.IsNullOrEmpty(this.Values))
            {
                this.values.AddRange(this.Values.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
            }

            // check the minimum number allowed, add extra fields.
            if (this.values.Count < this.Options.Minimum && this.Options.Minimum > 1)
            {
                this.values.AddRange(new string(',', this.Options.Minimum - 1).Split(new[] { ',' }, StringSplitOptions.None));
            }

            // check the maxmimum number allowed, remove the excess.
            if (this.values.Count > this.Options.Maximum && this.Options.Maximum > 0)
            {
                this.values.RemoveRange(this.Options.Maximum, this.values.Count - this.Options.Maximum);
            }

            // if there are no selected values...
            if (this.values.Count == 0)
            {
                // ... then add an empty string to display a single textstring box.
                this.values.Add(string.Empty);
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.EnsureChildControls();

            // populate the control's attributes.
            this.SelectedValues.ID = this.SelectedValues.ClientID;

            // add the controls.
            this.Controls.Add(this.SelectedValues);
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "MultipleTextstring");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // loop through each value
            foreach (string value in this.values)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "textstring-row");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                // input tag
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "textstring-row-field");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteLine("<input type='text' class='umbEditorTextField' value='{0}' />", value.Replace("'", "&#39;"));

                // append the add/remove buttons
                writer.WriteLine(" <a href='#add' class='add_row' title='Add a new row'><img src='{0}/images/small_plus.png' /></a>", GlobalSettings.Path);
                writer.WriteLine(" <a href='#remove' class='remove_row' title='Remove this row'><img src='{0}/images/small_minus.png' /></a>", GlobalSettings.Path);
                writer.RenderEndTag(); // </div> .textstring-row-field

                writer.WriteLine("<div class='textstring-row-sort' title='Re-order this row' style='background: url({0}/images/sort.png) no-repeat 0 2px;'></div>", GlobalSettings.Path);

                writer.RenderEndTag(); // </div> .textstring-row
            }

            this.SelectedValues.RenderControl(writer);

            writer.RenderEndTag(); // </div> .MultipleTextstring

            // add jquery window load event
            var javascriptMethod = string.Format("jQuery('#{0}').MultipleTextstring('#{1}', {2}, {3});", this.ClientID, this.SelectedValues.ClientID, this.Options.Minimum, this.Options.Maximum);
            var javascript = string.Concat("<script type='text/javascript'>jQuery(window).load(function(){", javascriptMethod, "});</script>");
            writer.WriteLine(javascript);
        }
    }
}