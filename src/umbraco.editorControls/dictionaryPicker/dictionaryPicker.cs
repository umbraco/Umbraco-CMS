using System;
using System.Collections;
using System.Web.UI.WebControls;

namespace umbraco.editorControls.dictionaryPicker
{
	/// <summary>
	/// Summary description for dictionaryPicker.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class dictionaryPicker : System.Web.UI.WebControls.CheckBoxList, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		private SortedList _prevalues;
		private string _text;

		public dictionaryPicker(interfaces.IData Data, SortedList Prevalues) 
		{
			_data = Data;
			_prevalues = Prevalues;
		}
		#region IDataEditor Members
		public override String Text 
		{
			get 
			{
				if (_data != null && _data.Value != null)
					return _data.Value.ToString();
				else
					return "";
				
			
			}
			set {_text = value;}
		}

		public bool ShowLabel
		{
			get
			{
				// TODO:  Add dictionaryPicker.ShowLabel getter implementation
				return true;
			}
		}

		public System.Web.UI.Control Editor
		{
			get
			{
				return this;
			}
		}

		public void Save()
		{
			_text = "";
			foreach(ListItem li in base.Items) 
			{
				if (li.Selected) 
				{
					_text += li.Value + ",";
				}
			}

			if (_text.Length > 0)
				_text = _text.Substring(0, _text.Length-1);
			_data.Value = _text;
		}

		public bool TreatAsRichTextEditor
		{
			get
			{
				// TODO:  Add dictionaryPicker.TreatAsRichTextEditor getter implementation
				return false;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

			if (_prevalues.Keys.Count > 0) 
			{
				// Find associated domain
				int languageId = 0;
                cms.businesslogic.web.Domain[] domains = library.GetCurrentDomains(((umbraco.cms.businesslogic.datatype.DefaultData)_data).NodeId);
				if (domains != null) 
					if (domains.Length > -1) 
						languageId = domains[0].Language.id;


				string key = _prevalues.GetByIndex(0).ToString();
				addDictionaries("", key, languageId);
			}
		}

		private void addDictionaries(string indent, string key, int language) 
		{
			cms.businesslogic.Dictionary.DictionaryItem di = new cms.businesslogic.Dictionary.DictionaryItem(key);

			foreach(cms.businesslogic.Dictionary.DictionaryItem item in di.Children) 
			{
				ListItem li;
				if (language != 0)
					li = new ListItem(indent + " " + item.Value(language), item.key);
				else
					li = new ListItem(indent + " " + item.Value(), item.key);

				if ((","+Text+",").IndexOf(","+li.Value.ToString()+",") > -1 && !Page.IsPostBack)
					li.Selected = true;
				this.Items.Add(li);
				addDictionaries(indent + "--", item.key, language);
			}
		}


		#endregion
	}
}
