using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.UI.JavaScript
{
    public sealed class ServerVariablesParser
    {

        /// <summary>
        /// Could allow developers to add custom variables on startup
        /// </summary>
        public static event EventHandler<Dictionary<string, object>> Parsing;

        internal const string Token = "##Variables##";

        internal static string Parse(Dictionary<string, object> items)
        {
            var vars = Resources.ServerVariables;

            if (Parsing != null)
            {
                Parsing(null, items);
            }

            var json = JObject.FromObject(items);
            return vars.Replace(Token, json.ToString());  
          
        }
        
    }
}