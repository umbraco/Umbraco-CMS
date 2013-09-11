using System;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.controls;
using umbraco.editorControls.UrlPicker.AjaxUpload;
using umbraco.editorControls.UrlPicker.Dto;
using umbraco.editorControls.UrlPicker.Services;
using umbraco.uicontrols.TreePicker;

[assembly: WebResource("umbraco.editorControls.UrlPicker.UrlPickerScripts.js", "application/x-javascript")]
[assembly: WebResource("umbraco.editorControls.UrlPicker.UrlPickerStyles.css", "text/css", PerformSubstitution = true)]

namespace umbraco.editorControls.UrlPicker
{
    /// <summary>
    /// The DataEditor for the UrlPicker.
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "ui/json2.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.form.js", "UmbracoClient")]
    [ValidationProperty("Url")]
    public class UrlPickerDataEditor : Panel
    {
        /// <summary>
        /// Creates the service/handler files on the user's system so Ajax can be done
        /// </summary>
        private void EnsureAjaxInfrastructure()
        {
            // AjaxUploadHandler
            AjaxUploadHandler.Ensure();

            // UrlPickerService
            UrlPickerService.Ensure();
        }

        /// <summary>
        /// Stores the state of the control in the ViewState via this control ONLY
        /// </summary>
        protected HiddenField StateHiddenField = new HiddenField();

        /// <summary>
        /// The control for the <c>ContentPicker</c>.
        /// </summary>
        protected SimpleContentPicker ContentPicker = new SimpleContentPicker();

        /// <summary>
        /// The control for the <c>MediaPicker</c>.
        /// </summary>
        protected SimpleMediaPicker MediaPicker = new SimpleMediaPicker();

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
                throw new InvalidOperationException("Settings must be set by Init on the UrlPickerDataEditor");
            }
            else if (Settings.AllowedModes.Count() == 0)
            {
                throw new InvalidOperationException("No modes have been allowed for the URL picker. See the developer section.");
            }

            this.EnsureAjaxInfrastructure();
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
            this.RegisterEmbeddedClientResource("umbraco.editorControls.UrlPicker.UrlPickerStyles.css", umbraco.cms.businesslogic.datatype.ClientDependencyType.Css);
            this.RegisterEmbeddedClientResource("umbraco.editorControls.UrlPicker.UrlPickerScripts.js", umbraco.cms.businesslogic.datatype.ClientDependencyType.Javascript);

            // If this.State was not set, create a default state
            if (State == null)
            {
                State = new UrlPickerState(Settings);
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Content Mode:
            ContentPicker.ID = ContentPicker.ClientID;
            ContentPicker.EnableViewState = false;
            this.Controls.Add(ContentPicker);

            // Media Mode:
            MediaPicker.ID = MediaPicker.ClientID;
            MediaPicker.EnableViewState = false;
            this.Controls.Add(MediaPicker);

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
            writer.AddAttribute("class", "ucomponents-url-picker");
            writer.AddAttribute("id", this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            StateHiddenField.RenderControl(writer);

            writer.Write("<label class='title'>{0}: <input type='text' /></label>", "Title");
            writer.Write("<label class='new-window'>{0} <input type='checkbox' /></label>", "Open in new window?");

            // Render allowed views.  Let JS/CSS control
            // visibility.
            writer.Write(@"<ul class='mode-chooser'>");
            foreach (UrlPickerMode mode in Settings.AllowedModes)
            {
                writer.Write(string.Format(@"
                    <li data-mode='{0}'>
                        {1}
                    </li>",
                    (int)mode,
                    mode.ToString()
                ));
            }
            writer.Write(@"</ul>");

            // Render views
            writer.Write(@"<ul class='mode-views'>");
            {
                // URL
                writer.Write(string.Format(@"
                    <li data-mode='{0}' class='{1}'>
                        <input type='text' />
                    </li>",
                    (int)UrlPickerMode.URL,
                    UrlPickerMode.URL.ToString().ToLower()
                ));

                // Content
                writer.Write(string.Format(@"
                    <li data-mode='{0}' data-id='{2}' class='{1}'>",
                    (int)UrlPickerMode.Content,
                    UrlPickerMode.Content.ToString().ToLower(),
                    ContentPicker.ClientID
                ));
                ContentPicker.RenderControl(writer);
                writer.Write("</li>");

                // Media
                writer.Write(string.Format(@"
                    <li data-mode='{0}' data-id='{2}' class='{1}'>",
                    (int)UrlPickerMode.Media,
                    UrlPickerMode.Media.ToString().ToLower(),
                    MediaPicker.ClientID
                ));
                MediaPicker.RenderControl(writer);
                writer.Write("</li>");

                // Upload
                writer.Write(string.Format(@"
                    <li data-mode='{0}' class='{1}'>
                        <div class='upload-view'>
                            <input type='file' />
                            <input type='button' value='{2}' class='upload-button' />
                        </div>
                        <div class='status-view'>
                            <a class='file-url' href=''></a>
                            <input type='button' value='{3}' class='delete-button' />
                        </div>
                    </li>",
                    (int)UrlPickerMode.Upload,
                    UrlPickerMode.Upload.ToString().ToLower(),
                    "Upload",
                    "Delete"
                ));
            }
            writer.Write(@"</ul>");

            //add scripts
            if (!Settings.Standalone)
            {
                var jss = new JavaScriptSerializer();
                writer.WriteLine(string.Format(@"
                    <script type='text/javascript'>
                        jQuery(function() {{
                            jQuery('#{0}').UrlPicker({{
                                state: {1},
                                settings: {2},
                                change: function (state) {{
                                    $('#' + '{3}').val(JSON.stringify(state));
                                }}
                            }});
                        }});
                    </script>",
                    this.ClientID,
                    jss.Serialize(State),
                    jss.Serialize(Settings),
                    StateHiddenField.ClientID
                ));
            }

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets or sets the state of the entire control
        /// </summary>
        /// <value>The state.</value>
        public UrlPickerState State
        {
            get
            {
                return UrlPickerState.Deserialize(StateHiddenField.Value);
            }

            set
            {
                // Is the mode allowed?  If not, set state to default - this will reset it on save/publish
                if (Settings.AllowedModes.Any(x => x == value.Mode))
                {
                    StateHiddenField.Value = value.Serialize(UrlPickerDataFormat.Json);
                }
                else
                {
                    var defaultState = new UrlPickerState(Settings);

                    // Set to first allowed mode
                    defaultState.Mode = Settings.AllowedModes.First();

                    StateHiddenField.Value = defaultState.Serialize(UrlPickerDataFormat.Json);
                }
            }
        }

        /// <summary>
        /// Settings defined for this UrlPicker
        /// </summary>
        public UrlPickerSettings Settings { get; set; }

        /// <summary>
        /// Used for "Mandatory" behaviour
        /// </summary>
        public string Url
        {
            get
            {
                return State != null ? State.Url : string.Empty;
            }
        }
    }
}