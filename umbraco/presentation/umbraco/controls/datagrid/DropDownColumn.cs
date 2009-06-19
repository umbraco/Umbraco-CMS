using System;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Data;


namespace umbraco.controls.datagrid
{
	public class DropDownColumn : DataGridColumn
	{
		private ListItemCollection _datasource = new ListItemCollection();
		private string _datafield;
		private string _dataValueField;
		
		public ListItemCollection Items 
		{
			get 
			{
				return _datasource;
			}
			set 
			{
				_datasource = value;
			}
		}

		public string DataField 
		{
			get 
			{
				return _datafield;
			}
			set 
			{
				_datafield = value;
			}
		}	

		public string DataValueField 
		{
			get 
			{
				return _dataValueField;
			}
			set 
			{
				_dataValueField = value;
			}
		}
		
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell (cell, columnIndex, itemType);
			switch (itemType) 
			{
				case ListItemType.Header :
					cell.Text = HeaderText;
					break;
				
				case ListItemType.AlternatingItem :
				case ListItemType.Item :
				case ListItemType.EditItem :
					cell.DataBinding += new System.EventHandler(EditItemDataBinding);
					DropDownList DDL = new DropDownList();
					cell.Controls.Add(DDL);
					break;
			}
		}
		
		private void ItemDataBinding(object sender, System.EventArgs e) 
		{
			TableCell cell  = (TableCell) sender;
			DataGridItem DGI = (DataGridItem) cell.NamingContainer;
			cell.Text = ((DataRowView) DGI.DataItem)[DataField].ToString();
		}
		
		private void EditItemDataBinding(object sender, System.EventArgs e) 
		{
			TableCell cell  = (TableCell) sender;
			DataGridItem DGI = (DataGridItem) cell.NamingContainer;
			DropDownList DDL  = (DropDownList) cell.Controls[0];
			string selectedValue = "";
			
			try 
			{
			
			} 
			catch {}

			foreach (ListItem li in Items) 
			{
				if (li.Value == selectedValue)
					li.Selected = true;
				DDL.Items.Add(li);
			}
		}
	}
}