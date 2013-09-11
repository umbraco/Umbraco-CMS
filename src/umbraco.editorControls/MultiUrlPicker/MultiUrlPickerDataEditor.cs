using System;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.editorControls.MultiUrlPicker.Dto;
using umbraco.editorControls.UrlPicker;

[assembly: WebResource("umbraco.editorControls.MultiUrlPicker.MultiUrlPickerScripts.js", "application/x-javascript")]
[assembly: WebResource("umbraco.editorControls.MultiUrlPicker.MultiUrlPickerStyles.css", "text/css")]

namespace umbraco.editorControls.MultiUrlPicker
{
    /// <summary>
    /// The DataEditor for the MultiUrlPicker.
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "ui/json2.js", "UmbracoClient")]
    public class MultiUrlPickerDataEditor : Panel
    {
        /// <summary>
        /// The Url Picker data editor, which is shared
        /// </summary>
        protected UrlPickerDataEditor UrlPicker;

        /// <summary>
        /// Stores the state of the control in the ViewState via this control ONLY
        /// </summary>
        protected HiddenField StateHiddenField;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Ensure settings are set by external entity
            if (Settings == null)
            {
                throw new InvalidOperationException("Settings must be set by Init on the MultiUrlPickerDataEditor");
            }

            this.EnsureChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //add the js/css required
            this.RegisterEmbeddedClientResource("umbraco.editorControls.MultiUrlPicker.MultiUrlPickerStyles.css", umbraco.cms.businesslogic.datatype.ClientDependencyType.Css);
            this.RegisterEmbeddedClientResource("umbraco.editorControls.MultiUrlPicker.MultiUrlPickerScripts.js", umbraco.cms.businesslogic.datatype.ClientDependencyType.Javascript);

            // If this.State was not set, create a default state
            if (State == null)
            {
                State = new MultiUrlPickerState();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Create the shared URL picker
            UrlPicker = new UrlPickerDataEditor();
            UrlPicker.ID = UrlPicker.ClientID;
            UrlPicker.EnableViewState = false;
            UrlPicker.Settings = this.Settings.UrlPickerSettings;
            this.Controls.Add(UrlPicker);

            // Create the hidden field to hold state
            StateHiddenField = new HiddenField();
            StateHiddenField.ID = StateHiddenField.ClientID;
            StateHiddenField.EnableViewState = true;
            this.Controls.Add(StateHiddenField);
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // Multi-purpose serializer
            var jss = new JavaScriptSerializer();

            writer.AddAttribute("class", "ucomponents-multi-url-picker");
            writer.AddAttribute("id", this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            StateHiddenField.RenderControl(writer);

            writer.Write("<ul class='items'> </ul>");
            writer.Write("<a class='add' href='#'>{0}</a>", "Add Item");

            // Set up JS (only if this ain't a standalone DataEditor)
            if (!Settings.Standalone)
            {
                UrlPicker.RenderControl(writer);
                writer.WriteLine(string.Format(@"
                    <script type='text/javascript'>
                        jQuery(function() {{
                            jQuery('#{0}').MultiUrlPicker({{
                                state: {1},
                                settings: {2},
                                urlPickerId: '{3}',
                                change: function (state) {{
                                    $('#' + '{4}').val(JSON.stringify(state));
                                }}
                            }});
                        }});
                    </script>",
                    this.ClientID,
                    jss.Serialize(State),
                    jss.Serialize(Settings),
                    UrlPicker.ClientID,
                    StateHiddenField.ClientID
                ));
            }

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets or sets the state of the entire control
        /// </summary>
        /// <value>The state.</value>
        public MultiUrlPickerState State
        {
            get
            {
                return MultiUrlPickerState.Deserialize(StateHiddenField.Value);
            }
            set
            {
                StateHiddenField.Value = value.Serialize(UrlPickerDataFormat.Json);
            }
        }

        /// <summary>
        /// Settings defined for this MultiUrlPicker
        /// </summary>
        public MultiUrlPickerSettings Settings { get; set; }
    }
}