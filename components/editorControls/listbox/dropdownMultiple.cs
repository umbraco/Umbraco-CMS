using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for dropdownMultiple.
	/// </summary>
	public class dropdownMultiple : System.Web.UI.WebControls.ListBox, interfaces.IDataEditor	
	{
		private String _text;

		interfaces.IData _data;
		SortedList _prevalues;
		public dropdownMultiple(interfaces.IData Data,SortedList Prevalues)
		{
			_data = Data;
			_prevalues = Prevalues;
		}

		public Control Editor { get {return this;}}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public virtual bool ShowLabel 
		{
			get {return true;}
		}

		//public override String Text 
		//{
		//     get {
				
		//          return _data.Value.ToString();
			
		//     }
		//     set {_text = value;}
		//}

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

		protected override void OnInit(EventArgs e)
		{
			base.SelectionMode = System.Web.UI.WebControls.ListSelectionMode.Multiple;
			
			if (_data != null && _data.Value != null)
				_text = _data.Value.ToString();

			base.OnInit(e);

            foreach (object key in _prevalues.Keys) 
			{
				ListItem li = new ListItem(_prevalues[key].ToString(),key.ToString());
				
				if ((","+_text+",").IndexOf(","+li.Value.ToString()+",") > -1)
					li.Selected = true;
					
				Items.Add(li);
			}
		}
	}
}
