using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// A much easier way to build custom datatypes. Inherit from this class
    /// and change Id and DataTypeName and then set the RenderControl property
    /// to your .NET webcontrol to be displayed
    /// Made by NH in flight SK 925 on his way to MIX09
    /// </summary>
    public abstract class AbstractDataEditor : BaseDataType, interfaces.IDataType
    {
        private interfaces.IData _baseData;
        private interfaces.IDataPrevalue _prevalueeditor;
        private AbstractDataEditorControl m_editor;

        public AbstractDataEditor()
        {
            m_editor = new AbstractDataEditorControl(this);
        }
        /// <summary>
        /// The data editor control is the 'real' IDataEditor control. Hook into the
        /// OnSave event in your inherited class' constructor and update the
        /// base.Data.Value property to save a value
        /// </summary>
        /// <value>The data editor control.</value>
        public AbstractDataEditorControl DataEditorControl
        {
            get { return m_editor; }
        }

        #region IDataEditor Members
        /// <summary>
        /// The RenderControl the control that's rendered when editing. This should be 
        /// *your* control, so set this property to the WebControl that you're creating
        /// </summary>
        /// <value>The render control.</value>
        public System.Web.UI.Control RenderControl
        {
            set
            {
                m_editor.Control = value;
            }
        }

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get { return m_editor; }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new DefaultData(this);
                return _baseData;
            }
        }

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueeditor == null)
                    _prevalueeditor = new DefaultPreValueEditor(this, false);
                return _prevalueeditor;
            }
        }

        public override string DataTypeName
        {
            get { throw new NotImplementedException(); }
        }

        public override Guid Id
        {
            get { throw new NotImplementedException(); }
        }

    }

    [ValidationProperty("Value")]
    public class AbstractDataEditorControl : System.Web.UI.WebControls.WebControl, interfaces.IDataEditor
    {
        private cms.businesslogic.datatype.BaseDataType _datatype;

        // property is used as a wrapper around the actual controls ValidationProperty
        public object Value {

            get
            {
               
                var attr = this.Control.GetType().GetCustomAttributes(typeof(ValidationPropertyAttribute), true);

                if (attr.Length > 0)
                {
                    //get value of marked property
                    System.Reflection.PropertyInfo info = this.Control.GetType().GetProperty(((ValidationPropertyAttribute)attr[0]).Name);
                    return info.GetValue(this.Control, null);
                }
                else
                {
                    //not marked so no validation
                    return "ok";
                }
                
               
            }
        
        }
        public AbstractDataEditorControl(cms.businesslogic.datatype.BaseDataType DataType)
        {
            _datatype = DataType;
        }

        public System.Web.UI.Control Control
        {
            get;
            set;
        }

        public virtual void Save()
        {
            FireOnSave(new EventArgs());
        }

        public virtual bool ShowLabel
        {
            get { return true; }
        }

        [Obsolete("This is legacy and should be left to false")]
        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public virtual System.Web.UI.Control Editor
        {
            get { return this; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (_datatype.HasSettings())
            {
                DataEditorSettingsStorage ss = new DataEditorSettingsStorage();

                List<Setting<string, string>> s = ss.GetSettings(_datatype.DataTypeDefinitionId);
                ss.Dispose();

                _datatype.LoadSettings(s);
            }

            this.Controls.Add(Control);
        }

        #endregion

        // events


        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(EventArgs e);
        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public event SaveEventHandler OnSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireOnSave(EventArgs e)
        {
            if (OnSave != null)
            {
                OnSave(e);
            }
        }

    }
}
