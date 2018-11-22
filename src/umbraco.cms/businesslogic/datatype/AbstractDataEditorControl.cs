namespace umbraco.cms.businesslogic.datatype
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using umbraco.interfaces;

    /// <summary>
    /// Base class that defines the methods, properties and events for all datatype controls.
    /// </summary>
    [ValidationProperty("Value")]
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class AbstractDataEditorControl : WebControl, INamingContainer, IDataEditor
    {
        /// <summary>
        /// The base datatype.
        /// </summary>
        private readonly BaseDataType datatype;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataEditorControl" /> class.
        /// </summary>
        /// <param name="datatype">The base datatype.</param>
        public AbstractDataEditorControl(BaseDataType datatype)
        {
            this.datatype = datatype;
        }

        /// <summary>
        /// The save event handler
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public delegate void SaveEventHandler(EventArgs e);

        /// <summary>
        /// Occurs when [on save].
        /// </summary>
        public event SaveEventHandler OnSave;

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        /// <value>
        /// The control.
        /// </value>
        public Control Control { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value
        {
            get
            {
                if (this.Control != null) 
                {
                    var attr = this.Control.GetType().GetCustomAttributes(typeof(ValidationPropertyAttribute), true);

                    if (attr.Length > 0)
                    {
                        // get value of marked property
                        var info = this.Control.GetType().GetProperty(((ValidationPropertyAttribute)attr[0]).Name);

                        return info.GetValue(this.Control, null);
                    }
                }

                // not marked so no validation
                return "ok";
            }
        }

        /// <summary>
        /// Gets a value indicating whether a label is shown
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show label]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ShowLabel
        { 
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the editor should be treated as a rich text editor.
        /// </summary>
        /// <value>
        /// <c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("This is legacy and should be left to false")]
        public virtual bool TreatAsRichTextEditor
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the editor control.
        /// </summary>
        /// <value>
        /// The editor.
        /// </value>
        public virtual Control Editor
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Saves the data in this control.
        /// </summary>
        public virtual void Save()
        {
            this.FireOnSave(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.datatype.HasSettings())
            {
                var ss = new DataEditorSettingsStorage();

                var s = ss.GetSettings(this.datatype.DataTypeDefinitionId);
                ss.Dispose();

                this.datatype.LoadSettings(s);
            }

            this.Controls.Add(this.Control);
        }

        /// <summary>
        /// Raises the <see cref="OnSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireOnSave(EventArgs e)
        {
            if (this.OnSave != null)
            {
                this.OnSave(e);
            }
        }
    }
}