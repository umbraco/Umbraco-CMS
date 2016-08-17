using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web._Legacy.Actions;

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
			data.Add("label", ApplicationContext.Current.Services.TextService.Localize(a.Alias));

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
				foreach (var a in ActionsResolver.Current.Actions)
				{
					types.Add(a.GetType());
				}
				return types;
			}


		}
	}
}
