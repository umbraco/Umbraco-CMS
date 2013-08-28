using System;
using System.Web.Razor;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;

namespace umbraco.controls
{
	/// <summary>
	/// Summary description for dualSelectbox.
	/// </summary>
	[ClientDependency(ClientDependencyType.Javascript, "js/dualSelectBox.js", "UmbracoRoot")]
	public class DualSelectbox : System.Web.UI.WebControls.WebControl, System.Web.UI.INamingContainer
	{
		private ListItemCollection _items = new ListItemCollection();

		private ListBox _possibleValues = new ListBox();
		private ListBox _selectedValues = new ListBox();
		private HtmlInputHidden _value = new HtmlInputHidden();
        private HtmlInputButton _add = new HtmlInputButton();
        private HtmlInputButton _remove = new HtmlInputButton();
		private int _rows = 8;


		public ListItemCollection Items 
		{
			get 
			{
				EnsureChildControls();
				return _items;
			}
		}

		public new int Width 
		{
			set 
			{
				_possibleValues.Width = new Unit(value);
				_selectedValues.Width = new Unit(value);
			}
		}

        protected override void CreateChildControls()
        {
            _possibleValues.ID = "posVals";
            _selectedValues.ID = "selVals";
            _possibleValues.SelectionMode = ListSelectionMode.Multiple;
            _selectedValues.SelectionMode = ListSelectionMode.Multiple;
            _possibleValues.CssClass = "guiInputTextStandard";
            _selectedValues.CssClass = "guiInputTextStandard";
            _possibleValues.Rows = _rows;
            _selectedValues.Rows = _rows;

            _value.ID = "theValue";

            HtmlTable table = new HtmlTable();
            table.CellPadding = 5;
            table.CellSpacing = 0;
            table.Border = 0;

            HtmlTableRow header = new HtmlTableRow();
            header.Controls.Add(new HtmlTableCell { InnerHtml = ui.Text("content", "notmemberof") });
            header.Controls.Add(new HtmlTableCell { InnerHtml= "&nbsp;" });
            header.Controls.Add(new HtmlTableCell { InnerHtml = ui.Text("content", "memberof") });
            table.Controls.Add(header);

            HtmlTableRow row = new HtmlTableRow();
            table.Controls.Add(row);
            HtmlTableCell cFirst = new HtmlTableCell();
            cFirst.Controls.Add(_possibleValues);
            row.Controls.Add(cFirst);
            HtmlTableCell cButtons = new HtmlTableCell();
            _add.Value = ">>";
            _add.Attributes.Add("class", "guiInputButton");
            _remove.Value = "<<";
            _remove.Attributes.Add("class", "guiInputButton");
            cButtons.Controls.Add(_add);
            cButtons.Controls.Add(new LiteralControl("<br/><br/>"));
            cButtons.Controls.Add(_remove);
            row.Controls.Add(cButtons);
            HtmlTableCell cSecond = new HtmlTableCell();
            cSecond.Controls.Add(_selectedValues);
            row.Controls.Add(cSecond);

            this.Controls.Add(table);
            this.Controls.Add(_value);
        }

		public string Value 
		{
			get 
			{
                return _value.Value;
			}
			
			set 
			{
                _value.Value = value;
            }
		}

		public int Rows 
		{
			set 
			{
				_rows = value;
			}
		}

		public DualSelectbox()
		{
		}


		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			_selectedValues.Items.Clear();
			_possibleValues.Items.Clear();

			foreach(ListItem li in _items) 
			{
				if (((string) (","+ this.Value +",")).IndexOf(","+li.Value+",") > -1) 
					_selectedValues.Items.Add(li);
				else
					_possibleValues.Items.Add(li);											
			}

            // add js to buttons here to ensure full clientids
            _add.Attributes.Add("onClick", "dualSelectBoxShift('" + this.ClientID + "');");
            _remove.Attributes.Add("onClick", "dualSelectBoxShift('" + this.ClientID + "');");
        }

		


	}
}
