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
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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

            // zb-00024 #299260 : see the loooong comments in OnLoad
            // setting the value here seems OK but use this.Value to ensure
            // that the hidden control exists
            this.Value = StoredItemId != -1 ? StoredItemId.ToString() : "";

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //need to check if this is an async postback in live editing, because if it is, we need to set the value
            //SD: Since live editing is removed I don't really think this is at all necessary but it's still here.
            if ((ScriptManager.GetCurrent(Page).IsInAsyncPostBack) || !Page.IsPostBack)
            {
                ItemIdValue.Value = StoredItemId != -1 ? StoredItemId.ToString() : "";
            }

            // zb-00024 #299260 : support postback
            // on postback ItemIdValue.Value is restored by LoadPostData
            // on non-postback it is initialized by the code above
            // w/RCC when clicking on 'edit' it is not initialized

            // StoredItemId is initialized in OnInit with whatever comes from the
            // database, or from what the RCC decides we need to display--but when
            // we changes something in the web page, it is handled by LoadPostData

            // RCC 'edit':
            // ItemIdValue.Value has no value, or a wrong value, which we should ignore
            // StoredItemId is initialized with the right value in RCC code
            // so we'd need to set ItemIdValue.Value from it

            // but if we do it in prerender then we copy over the /old/ StoredItemIt
            // value which was the one before we saved... because we don't refresh it
            // when we update

            // and if we refresh it here we are in trouble because we overwrite what
            // comes from the form and we can't actually change the value anymore

            // so we should change it /once/ the ItemIdValue.Value changes have been
            // akn

            // the viewed image ID is set in mediaChoose onPreRender from
            // ItemIdValue.Value so we really want it to have a value!
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
                _data.Value = int.Parse(ItemIdValue.Value.Trim());
            else
                _data.Value = null;
        }

        #endregion
    }
}
