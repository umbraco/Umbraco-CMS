using System;
using System.Collections.Generic;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.propertytype;

namespace umbraco.controls.GenericProperties
{
    /// <summary>
    /// Summary description for GenericPropertyWrapper.
    /// </summary>
    public class GenericPropertyWrapper : System.Web.UI.WebControls.PlaceHolder
    {
        private readonly bool _allowNameEdit = true;
        private readonly bool _allowDataTypeEdit = true;
        private readonly bool _allowAliasEdit = true;
        private readonly bool _allowTabEdit = true;
        private readonly bool _allowValidationEdit = true;
        private readonly bool _allowDescEdit = true;
        private readonly bool _allowDelete = true;

        private GenericProperty _gp;
        private cms.businesslogic.ContentType.TabI[] _tabs;
        private cms.businesslogic.datatype.DataTypeDefinition[] _dtds;
        private int _tabId;
        private string _fullId = "";

        public event EventHandler Delete;

        public PropertyType PropertyType { get; set; }

        public int TabId
        {
            set { _tabId = value; }
        }

        public cms.businesslogic.datatype.DataTypeDefinition[] DataTypeDefinitions
        {
            set { _dtds = value; }
        }

        public cms.businesslogic.web.DocumentType.TabI[] Tabs
        {
            set { _tabs = value; }
        }

        public string FullId
        {
            set { _fullId = value; }
        }

        public GenericProperty GenricPropertyControl
        {
            get { return _gp; }
        }

        public GenericPropertyWrapper()
            : this(true)
        {

        }

        public GenericPropertyWrapper(bool allowModification)
        {
            if (allowModification == false)
            {
                _allowAliasEdit = false;
                _allowValidationEdit = false;
                _allowDelete = false;
                _allowDataTypeEdit = false;
            }
        }

        public GenericPropertyWrapper(bool allowNameEdit, bool allowDataTypeEdit, bool allowAliasEdit, bool allowTabEdit, bool allowValidationEdit, bool allowDescEdit, bool allowDelete)
        {
            _allowNameEdit = allowNameEdit;
            _allowDataTypeEdit = allowDataTypeEdit;
            _allowAliasEdit = allowAliasEdit;
            _allowTabEdit = allowTabEdit;
            _allowValidationEdit = allowValidationEdit;
            _allowDescEdit = allowDescEdit;
            _allowDelete = allowDelete;
        }

        public void UpdateEditControl()
        {
            if (Controls.Count == 1)
            {
                var u = Controls[0];
                u.ID = ID + "_control";
                _gp = (GenericProperty)u;
                _gp.PropertyType = PropertyType;
                _gp.DataTypeDefinitions = _dtds;
                _gp.Tabs = _tabs;
                _gp.TabId = _tabId;
                _gp.FullId = _fullId;
            }
        }

        protected void GenericPropertyWrapper_Delete(object sender, EventArgs e)
        {
            Delete(this, new EventArgs());
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var u = (GenericProperty)Page.LoadControl(SystemDirectories.Umbraco + "/controls/genericProperties/GenericProperty.ascx");
            
            u.AllowAliasEdit = _allowAliasEdit;
            u.AllowDataTypeEdit = _allowDataTypeEdit;
            u.AllowDelete = _allowDelete;
            u.AllowDescriptionEdit = _allowDescEdit;
            u.AllowNameEdit = _allowNameEdit;
            u.AllowTabEdit = _allowTabEdit;
            u.AllowValidationEdit = _allowValidationEdit;

            u.ID = ID + "_control";

            if (_allowDelete)
            {
                u.Delete += GenericPropertyWrapper_Delete;
            }

            u.FullId = _fullId;
            Controls.Add(u);
            UpdateEditControl();
        }


    }
}
