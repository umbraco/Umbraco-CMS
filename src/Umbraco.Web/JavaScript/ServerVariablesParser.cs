using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.JavaScript
{
    public class ServerVariablesParser
    {
        /// <summary>
        /// Allows developers to add custom variables on parsing
        /// </summary>
        public static event EventHandler<Dictionary<string, object>> Parsing;

        internal const string Token = "##Variables##";

        internal static string Parse(Dictionary<string, object> items)
        {
            var vars = Resources.ServerVariables;

            //Raise event for developers to add custom variables
            Parsing?.Invoke(null, items);

            var json = JObject.FromObject(items);
            return vars.Replace(Token, json.ToString());
        }
    }
}
