using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Xml;
using System.Xml.XPath;
using System.Reflection;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for task.
	/// </summary>
	public partial class theTask : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.PlaceHolder dontRefresh;


		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			if (IsPostBack) 
			{
				int parentID = 0;
				int typeID = 0;
				string nodeName;

				// Load task settings
				XmlDocument createDef = new XmlDocument();
				XmlTextReader defReader = new XmlTextReader(Server.MapPath(GlobalSettings.Path+ "/config/create/UI.xml"));
				createDef.Load(defReader);
				defReader.Close();

				// Find nodeType task
				XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + nodeType.Value + "']");
				string taskAssembly = def.SelectSingleNode("./tasks/" + task.Value.ToLower()).Attributes.GetNamedItem("assembly").Value;
				string taskType = def.SelectSingleNode("./tasks/" + task.Value.ToLower()).Attributes.GetNamedItem("type").Value;

				try 
				{
					parentID = Convert.ToInt32(nodeID.Value);
				}
				catch {}
				// Parse parameters
				if (parameterName.Value != "" && parameterName.Value.IndexOf("---") >= 0) 
				{
					
					string tempID = parameterName.Value.Substring(0, parameterName.Value.IndexOf("---"));
					
					if (tempID.Length > 0) 
					{
						if (nodeID.Value == "init") 
							parentID = -1;
							else parentID = Convert.ToInt32(nodeID.Value);
						
						typeID = Convert.ToInt32(parameterName.Value.Substring(0, parameterName.Value.IndexOf("---")));
						nodeName = parameterName.Value.Substring(parameterName.Value.IndexOf("---")+4, parameterName.Value.Length-parameterName.Value.IndexOf("---")-4);
					} 
					else
						nodeName = parameterName.Value.Substring(parameterName.Value.IndexOf("---")+4, parameterName.Value.Length-parameterName.Value.IndexOf("---")-4);
				} 
				else 
				{
					nodeName = "";

					if (parameterName.Value != "")
						nodeName = parameterName.Value;

					if (nodeID.Value != "")
						parentID = Convert.ToInt32(nodeID.Value);
				}

				// Lets reflect...
				try 
				{
					// Create an instance of the type by loading it from the assembly
					Assembly assembly = Assembly.LoadFrom(Server.MapPath(GlobalSettings.Path + "/../bin/"+taskAssembly+".dll"));
					Trace.Write(taskAssembly, taskType);
					Type type = assembly.GetType(taskAssembly+"."+taskType);
					interfaces.ITask typeInstance = Activator.CreateInstance(type) as interfaces.ITask;
					dontRefresh.Controls.Clear();
					if (typeInstance != null) 
					{
						typeInstance.TypeID = typeID;
						typeInstance.ParentID = parentID;
						Trace.Warn("ID:" +parentID);
						typeInstance.Alias = nodeName;
						switch (task.Value.ToLower()) 
						{
							case "create" :
								typeInstance.UserId = this.getUser().Id;
								typeInstance.Save();
								break;
							case "delete" :
								Trace.Write("tasks", "Deleting " + parentID.ToString());
								typeInstance.Delete();
								// Users aren't deleted, so we refresh the parent instead
								if (nodeType.Value == "user")
									dontRefresh.Controls.Add(new LiteralControl("refreshParent = true;\ndontDelete = true;\n"));
								else
									dontRefresh.Controls.Add(new LiteralControl("refreshParent = true;\n"));

								
								break;
							default:
								Trace.Warn("tasks", "Unknown task: '" + task.Value.ToLower() + "'");
								break;
						}
					} 
					else 
					{						
						Trace.Warn("task", "Type doesn't exist or is not umbraco.ITask ('" + taskAssembly + "." + taskType + "')");
					}

				} 
				catch (Exception exp)
				{
					Trace.Warn("task", "Error creating type '" + taskAssembly + "." + taskType + "'", exp);
				}


			}
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.ID = "theTask";

		}
		#endregion
	}
}
