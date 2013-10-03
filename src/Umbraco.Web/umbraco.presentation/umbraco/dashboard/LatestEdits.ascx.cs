using umbraco.BusinessLogic;
using System;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.web;

namespace dashboardUtilities
{
	/// <summary>
	///		Summary description for LatestEdits.
	/// </summary>
	public partial class LatestEdits : System.Web.UI.UserControl
	{

		// Find current user
		private System.Collections.ArrayList printedIds = new System.Collections.ArrayList();
		private int count = 0;
        public int MaxRecords { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			if (MaxRecords == 0)
		        MaxRecords = 30;

			Repeater1.DataSource = Log.GetLogReader(User.GetCurrent(), LogTypes.Save, DateTime.Now.Subtract(new TimeSpan(7,0,0,0,0)), MaxRecords);
			Repeater1.DataBind();
		}

		public string PrintNodeName(object NodeId, object Date) 
		{
			if (!printedIds.Contains(NodeId) && count < MaxRecords) 
			{
				printedIds.Add(NodeId);
				try 
				{
                    Document d = new Document(int.Parse(NodeId.ToString()));										
					count++;
					return
                        "<a href=\"#/content/content/edit/" + NodeId.ToString() + "\" style=\"text-decoration: none\"><i class='icon icon-document'></i> " + d.Text + " -  " + umbraco.ui.Text("general", "edited", User.GetCurrent()) + " " + umbraco.library.ShortDateWithTimeAndGlobal(DateTime.Parse(Date.ToString()).ToString(), umbraco.ui.Culture(User.GetCurrent())) + "</a>";
				}
				catch {
					return "";
				}
				
			} else
				return "";
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion
	}
}
