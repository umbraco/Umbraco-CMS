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
	internal class JTreeContextMenuItem : JavaScriptConverter
	{

		/// <summary>
		/// Not implemented as we never need to Deserialize
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="type"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			//{
			//    "id": "L",
			//    "label": "Create",
			//    "icon": "create.png",
			//    "visible": function(NODE, TREE_OBJ) { if (NODE.length != 1) return false; return TREE_OBJ.check("creatable", NODE); },
			//    "action": function(NODE, TREE_OBJ) { TREE_OBJ.create(false, NODE); },
			//}


			IAction a = (IAction)obj;
			Dictionary<string, object> data = new Dictionary<string, object>();

			data.Add("id", a.Letter);
			data.Add("label", ui.Text(a.Alias));

			if (a.Icon.StartsWith("."))
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(string.Format("<div class='menuSpr {0}'>", a.Icon.TrimStart('.')));
				sb.Append("</div>");
				sb.Append("<div class='menuLabel'>");
				sb.Append(data["label"].ToString());
				sb.Append("</div>");
				data["label"] = sb.ToString();
			}
			else
			{
				data.Add("icon", a.Icon);
			}

			//required by jsTree
			data.Add("visible", JSONSerializer.ToJSONObject("function() {return true;}"));
			
            //The action handler is what is assigned to the IAction, but for flexibility, we'll call our onContextMenuSelect method which will need to return true if the function is to execute.
			data.Add("action", JSONSerializer.ToJSONObject("function(N,T){" + a.JsFunctionName + ";}"));

			return data;

		}

		/// <summary>
		/// TODO: Find out why we can't just return IAction as one type (JavaScriptSerializer doesn't seem to pick up on it)
		/// </summary>
		public override IEnumerable<Type> SupportedTypes
		{
			get
			{
				List<Type> types = new List<Type>();
				foreach (IAction a in global::umbraco.BusinessLogic.Actions.Action.GetAll())
				{
					types.Add(a.GetType());
				}
				return types;
			}


		}
	}
}
