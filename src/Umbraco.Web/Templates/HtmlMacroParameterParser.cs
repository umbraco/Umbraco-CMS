using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Macros;

namespace Umbraco.Web.Templates
{

    public sealed class HtmlMacroParameterParser
    {
        public HtmlMacroParameterParser()
        {

        }

        /// <summary>
        /// Parses out media UDIs from an html string based on embedded macro parameter values
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<Udi> FindUdisFromMacroParameters(string text)
        {
            // we already have a method for finding Macros within a Rich Text Editor using regexes: Umbraco.Web.Macros.MacroTagParser
            // it searches for 'text blocks' which I think are the old way of embedding macros in a template, to allow inline code - we're not interested in those in V8
            // but we are interested in the foundMacros, and in particular their parameters and values
            // (aside TODO: at somepoint we might want to track Macro usage?)
            // ideally we'd determine each type of parameter editor and call it's GetReferences implementation
            // however this information isn't stored in the embedded macro markup and would involve 'looking up' types for each parameter
            // (or we could change the macros markup to include details of the editor)
            // but pragmatically we don't really care what type of property editor it is if it stores media udis
            // we want to track media udis wherever they are used regardless of how they are picked?
            // so here we parse the parameters and check to see if they are in the format umb://media
            // see also https://github.com/umbraco/Umbraco-CMS/pull/8388

            List<string> macroParameterValues = new List<string>();
            MacroTagParser.ParseMacros(text, textblock => { }, (macroAlias, macroAttributes) => macroParameterValues.AddRange(macroAttributes.Values.ToList().Where(f=>f.StartsWith("umb://" + Constants.UdiEntityType.Media + "/"))));

            if (macroParameterValues.Any())
            {
                // we have media udis! - but we could also have a csv of multiple media udis items
                foreach (var macroParameterValue in macroParameterValues.Distinct())
                {
                    string[] potentialUdis = macroParameterValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var potentialUdi in potentialUdis.Distinct())
                    {
                       if(Udi.TryParse(potentialUdi, out var udi))
                        {
                            yield return udi;
                        }
                    }
                }
            }
            else
            {
                yield break;
            }           
        }
        /// <summary>
        /// Parses out media UDIs from Macro Grid Control Parameters
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<Udi> FindUdisFromGridControlMacroParameters(IEnumerable<GridValue.GridControl> macroValues)
        {
            List<string> macroParameterValues = new List<string>();
            foreach (var macroValue in macroValues)
            {
                //deserialise JSON of Macro Grid Control to a class 
                var gridMacro = macroValue.Value.ToObject<GridMacro>();
                //collect any macro parameters that contain the media udi format
                if (gridMacro != null && gridMacro.MacroParameters!=null && gridMacro.MacroParameters.Any())
                {
                    macroParameterValues.AddRange(gridMacro.MacroParameters.Values.ToList().Where(f => f.Contains("umb://" + Constants.UdiEntityType.Media + "/")));
                }
                }  
            if (macroParameterValues.Any())
            {
                // we have media udis! - but we could also have a csv of multiple items
                foreach (var macroParameterValue in macroParameterValues.Distinct())
                {
                    string[] potentialUdis = macroParameterValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var potentialUdi in potentialUdis.Distinct())
                    {
                        if (Udi.TryParse(potentialUdi, out var udi))
                        {
                            yield return udi;
                        }
                    }
                }
            }
            else
            {
                yield break;
            }
        }
        // poco class to deserialise the Json for a Macro Control
        private class GridMacro
        {
            [JsonProperty("macroAlias")]
            public string MacroAlias { get; set; }
            [JsonProperty("macroParamsDictionary")]
            public Dictionary<string,string> MacroParameters { get; set; }
        }
    }
}
