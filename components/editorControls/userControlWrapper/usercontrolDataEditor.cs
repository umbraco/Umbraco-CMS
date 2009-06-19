using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using umbraco.interfaces;
using umbraco.editorControls;

namespace umbraco.editorControls.userControlGrapper
{
    public class usercontrolDataEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataEditor
	{
		private umbraco.interfaces.IData _data;
        private string _usercontrolPath;


        public usercontrolDataEditor(umbraco.interfaces.IData Data, string UsercontrolPath)
        {
			_data = Data;
            _usercontrolPath = UsercontrolPath;
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}

		public bool ShowLabel 
		{
			get {return true;}
		}

		public Control Editor {get{return this;}}

		public void Save() 
		{
            IUsercontrolDataEditor uc =
                (IUsercontrolDataEditor)Controls[0] as IUsercontrolDataEditor;

			_data.Value = uc.value;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

            this.Controls.Add(
                new System.Web.UI.UserControl().LoadControl(_usercontrolPath));

            if (!Page.IsPostBack)
                ((IUsercontrolDataEditor)Controls[0] as IUsercontrolDataEditor).value = _data.Value;
              
		}

	}
}