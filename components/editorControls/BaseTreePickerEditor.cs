using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.uicontrols.TreePicker;
using umbraco.interfaces;
using System.Web.UI;
using umbraco.presentation;

namespace umbraco.editorControls
{
    /// <summary>
    /// A base tree picker class that has all of the functionality built in for an IDataEditor
    /// </summary>
    public abstract class BaseTreePickerEditor : BaseTreePicker, IDataEditor
    {

        interfaces.IData _data;
        protected int StoredItemId = -1;

        public BaseTreePickerEditor()
            : base() { }

        public BaseTreePickerEditor(IData Data)
            : base()
        {
            _data = Data;
        }

        private void StoreItemId(IData Data)
        {
            if (_data != null && _data.Value != null && !String.IsNullOrEmpty(_data.Value.ToString()))
            {
                int.TryParse(_data.Value.ToString(), out StoredItemId);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            StoreItemId(_data);

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //need to check if this is an async postback in live editing, because if it is, we need to set the value
            if ((ScriptManager.GetCurrent(Page).IsInAsyncPostBack 
                && UmbracoContext.Current.LiveEditingContext.Enabled)
                || !Page.IsPostBack)
            {
                ItemIdValue.Value = StoredItemId != -1 ? StoredItemId.ToString() : "";
            }
        }

        #region IDataField Members

        public System.Web.UI.Control Editor { get { return this; } }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }
        public bool ShowLabel
        {
            get
            {
                return true;
            }
        }

        public void Save()
        {
            if (ItemIdValue.Value.Trim() != "")
                _data.Value = ItemIdValue.Value.Trim();
            else
                _data.Value = null;
        }

        #endregion
    }
}
