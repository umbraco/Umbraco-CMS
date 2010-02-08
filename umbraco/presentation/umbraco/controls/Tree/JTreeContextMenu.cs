using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using umbraco.interfaces;
using System.Text.RegularExpressions;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic.Utils;
using System.Text;
using umbraco.cms.presentation.Trees;
using umbraco.BasePages;
using System.Web.Services;

namespace umbraco.controls.Tree
{
	internal class JTreeContextMenu
	{
		public string RenderJSONMenu()
		{

			JSONSerializer jSSerializer = new JSONSerializer();

			jSSerializer.RegisterConverters(new List<JavaScriptConverter>() 
					{ 	
						new JTreeContextMenuItem()
					});

			List<IAction> allActions = new List<IAction>();
			foreach (IAction a in global::umbraco.BusinessLogic.Actions.Action.GetAll())
			{
				if (!string.IsNullOrEmpty(a.Alias) && (!string.IsNullOrEmpty(a.JsFunctionName) || !string.IsNullOrEmpty(a.JsSource)))
				{
					allActions.Add(a);
				}

			}


			return jSSerializer.Serialize(allActions);
		}
	}
}
