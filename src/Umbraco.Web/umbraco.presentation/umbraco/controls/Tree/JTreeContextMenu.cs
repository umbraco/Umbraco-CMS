using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Umbraco.Core.Logging;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Web.LegacyActions;
using Umbraco.Web._Legacy.Utils;
using Action = Umbraco.Web.LegacyActions.Action;

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
			foreach (var a in ActionsResolver.Current.Actions)
			{
                // NH: Added a try/catch block to this as an error in a 3rd party action can crash the whole menu initialization
                try
                {
                    if (!string.IsNullOrEmpty(a.Alias) && (!string.IsNullOrEmpty(a.JsFunctionName) || !string.IsNullOrEmpty(a.JsSource)))
                    {
                        // if the action is using invalid javascript we need to do something about this
                        if (!Action.ValidateActionJs(a))
                        {
                            // Make new Iaction
                            PlaceboAction pa = new PlaceboAction(a);
                            pa.JsFunctionName = "IActionProxy_" + pa.Alias.ToSafeAlias() + "()";
                            allActions.Add(pa);

                        }
                        else
                        {
                            allActions.Add(a);
                        }
                    }
                }
                catch (Exception ee)
                {
	                LogHelper.Error<JTreeContextMenu>("Error initializing tree action", ee);
                }

			}


			return jSSerializer.Serialize(allActions);
		}
	}
}
