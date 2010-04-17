using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.editorControls
{
	public class dropdown : System.Web.UI.WebControls.DropDownList, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		private SortedList _prevalues;

		public dropdown(interfaces.IData Data, SortedList Prevalues) 
		{
			_data = Data;
			_prevalues = Prevalues;
		}

		public Control Editor {
			get {return this;}
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public virtual bool ShowLabel 
		{
			get {return true;}
		}

		public void Save() 
		{
			string tmpVal = "";
			if (this.SelectedIndex > 0)
				tmpVal = this.SelectedValue;
			_data.Value = tmpVal;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
            foreach (object key in _prevalues.Keys)
            {
				this.Items.Add(new ListItem(dropdown.DictionaryReplace(_prevalues[key].ToString()), key.ToString()));
			}
			base.Items.Insert(0, new ListItem(ui.Text("choose") + "...",""));

			if (_data != null && _data.Value != null)
				this.SelectedValue = _data.Value.ToString();			
		}

		static string DictionaryReplace(string text)
		{
			if (!text.StartsWith("#"))
				return text;
			else
			{
				Language lang = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
				if (lang != null)
				{
					if (Dictionary.DictionaryItem.hasKey(text.Substring(1, text.Length - 1)))
					{
						Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(text.Substring(1, text.Length - 1));
						return di.Value(lang.id);
					}
				}

				return "[" + text + "]";
			}
		}

	}
}