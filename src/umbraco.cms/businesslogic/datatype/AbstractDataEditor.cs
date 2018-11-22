namespace umbraco.cms.businesslogic.datatype
{
    using System;

    using umbraco.interfaces;

    /// <summary>
    /// A much easier way to build custom datatypes. Inherit from this class
    /// and change Id and DataTypeName and then set the RenderControl property
    /// to your .NET webcontrol to be displayed
    /// </summary>
    /// <remarks>
    /// Made by NH in flight SK 925 on his way to MIX09
    /// </remarks>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public abstract class AbstractDataEditor : BaseDataType, IDataType
    {
        /// <summary>
        /// The data editor.
        /// </summary>
        private readonly AbstractDataEditorControl editor;

        /// <summary>
        /// The stored data.
        /// </summary>
        private IData baseData;

        /// <summary>
        /// The prevalue editor.
        /// </summary>
        private IDataPrevalue prevalueEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataEditor" /> class.
        /// </summary>
        protected AbstractDataEditor()
        {
            this.editor = new AbstractDataEditorControl(this);
        }

        /// <summary>
        /// Gets the data editor control that is the 'real' IDataEditor control. 
        /// Hook into the OnSave event in your inherited class' constructor and update the base.Data.Value property to save a value.
        /// </summary>
        /// <value>The data editor control.</value>
        public AbstractDataEditorControl DataEditorControl
        {
            get { return this.editor; }
        }

        /// <summary>
        /// Sets the RenderControl to the control that's rendered when editing. 
        /// This should be *your* control, so set this property to the WebControl that you're creating.
        /// </summary>
        /// <value>
        /// The render control.
        /// </value>
        public System.Web.UI.Control RenderControl
        {
            set
            {
                this.editor.Control = value;
            }
        }

        /// <summary>
        /// Gets the data editor.
        /// </summary>
        /// <value>
        /// The data editor.
        /// </value>
        public override IDataEditor DataEditor
        {
            get { return this.editor; }
        }

        /// <summary>
        /// Gets the stored data.
        /// </summary>
        /// <value>
        /// The stored data.
        /// </value>
        public override IData Data
        {
            get
            {
                return this.baseData ?? (this.baseData = new DefaultData(this));
            }
        }

        /// <summary>
        /// Gets the prevalue editor.
        /// </summary>
        /// <value>
        /// The prevalue editor.
        /// </value>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                return this.prevalueEditor ?? (this.prevalueEditor = new DefaultPreValueEditor(this, false));
            }
        }

        /// <summary>
        /// Gets the name of the datatype.
        /// </summary>
        /// <value>
        /// The name of the datatype.
        /// </value>
        public abstract override string DataTypeName { get; }

        /// <summary>
        /// Gets the unique id of the datatype.
        /// </summary>
        /// <value>
        /// The unique id of the datatype.
        /// </value>
        public abstract override Guid Id { get; }
    }
}
