using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Templates
{

    public sealed class HtmlMacroParameterParser
    {
        private readonly IMacroService _macroService;
        private readonly ILogger _logger;
        //private readonly ParameterEditorCollection _parameterEditors;
        public HtmlMacroParameterParser(IMacroService macroService, ILogger logger)//, ParameterEditorCollection parameterEditorCollection)
        {
            //I get an error at boot injecting this here, what would be the best way of referencing it? 
            //_parameterEditors = parameterEditorCollection;
          _macroService = macroService;
          _logger = logger;
        }

        private IEnumerable<UmbracoEntityReference> GetUmbracoEntityReferencesFromMacros(List<Tuple<string, Dictionary<string, string>>> macros)
        {
            var uniqueMacroAliases = macros.Select(f => f.Item1).Distinct();
            //TODO: tracking Macro references...? am finding the used Macros Udis here - but not sure what to do with them - eg seems like there should be a Related Macro relation type - but Relations don't accept 'Macro' as an option
            var foundMacroUmbracoEntityReferences = new List<UmbracoEntityReference>(); 
            //Get all the macro configs in one hit for these unique macro aliases - this is now cached with a custom cache policy
            var macroConfigs = _macroService.GetAll(uniqueMacroAliases.ToArray());
            //get the registered parametereditors with the implmementation to avoid looking up for each parameter
            // can we inject this instead of using Current?? - I get a boot error when I attempt this directly into the parser constructor - could/should it be passed in via the method?
            var parameterEditors = Current.ParameterEditors;
            foreach (var macro in macros)
            {
                var macroConfig = macroConfigs.FirstOrDefault(f => f.Alias == macro.Item1);
                foundMacroUmbracoEntityReferences.Add(new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Macro, macroConfig.Key)));
                // only do this if the macros actually have parameters
                if (macroConfig.Properties != null && macroConfig.Properties.Keys.Any(f=>f!= "macroAlias"))
                {
                    foreach (var umbracoEntityReference in GetUmbracoEntityReferencesFromMacroParameters(macro.Item1, macro.Item2, macroConfig, parameterEditors))
                    {
                        yield return umbracoEntityReference;
                    }
                }
            }
        }
        /// <summary>
        /// Finds media UDIs in Macro Parameter Values by calling the GetReference method for all the Macro Parameter Editors for a particular Macro
        /// </summary>
        /// <param name="macroAlias">The alias of the macro</param>
        /// <param name="macroParameters">The parameters for the macro a dictionary of key/value strings</param>
        /// <param name="macroConfig">The macro configuration for this particular macro - contains the types of editors used for each parameter</param>
        /// <param name="parameterEditors">A list of all the registered parameter editors used in the Umbraco implmentation - to look up the corresponding property editor for a macro parameter</param>
        /// <returns></returns>
        private IEnumerable<UmbracoEntityReference> GetUmbracoEntityReferencesFromMacroParameters(string macroAlias, Dictionary<string, string> macroParameters, IMacro macroConfig, ParameterEditorCollection parameterEditors)
        {
            var foundUmbracoEntityReferences = new List<UmbracoEntityReference>();
            foreach (var parameter in macroConfig.Properties)
            {                
                if (macroParameters.TryGetValue(parameter.Alias, out string parameterValue))
                {
                    var parameterEditorAlias = parameter.EditorAlias;
                    //lookup propertyEditor from core current ParameterEditorCollection
                    var parameterEditor = parameterEditors.FirstOrDefault(f => String.Equals(f.Alias,parameterEditorAlias,StringComparison.OrdinalIgnoreCase));
                    if (parameterEditor != null)
                    {
                        //get the ParameterValueEditor for this PropertyEditor (where the GetReferences method is implemented) - cast As IDataValueReference to determine if 'it is' implemented for the editor
                        IDataValueReference parameterValueEditor = parameterEditor.GetValueEditor() as IDataValueReference;
                        if (parameterValueEditor != null)
                        {
                            foreach (var entityReference in parameterValueEditor.GetReferences(parameterValue))
                            {
                                foundUmbracoEntityReferences.Add(entityReference);
                            }
                        }
                        else
                        {
                           _logger.Info<HtmlMacroParameterParser>("{0} doesn't have a ValueEditor that implements IDataValueReference", parameterEditor.Alias);
                        }
                    }
                }
            }
            return foundUmbracoEntityReferences;
        }
        /// <summary>
        /// Parses out media UDIs from an html string based on embedded macro parameter values
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromEmbeddedMacros(string text)
        {
            // we already have a method for finding Macros within a Rich Text Editor using regexes: Umbraco.Web.Macros.MacroTagParser
            // it searches for 'text blocks' which I think are the old way of embedding macros in a template, to allow inline code - we're not interested in those in V8
            // but we are interested in the foundMacros,
            // there may be more than one macro with the same alias on the page so using a tuple
            List<Tuple<string, Dictionary<string, string>>> foundMacros = new List<Tuple<string, Dictionary<string, string>>>();

            //though this legacy ParseMacros does find the macros it seems to lowercase the macro parameter alias - so making the dictionary case insensitive
            MacroTagParser.ParseMacros(text, textblock => { }, (macroAlias, macroAttributes) => foundMacros.Add(new Tuple<string, Dictionary<string, string>>(macroAlias, new Dictionary<string,string>(macroAttributes,StringComparer.OrdinalIgnoreCase))));
            foreach (var umbracoEntityReference in GetUmbracoEntityReferencesFromMacros(foundMacros))
            {
                yield return umbracoEntityReference;
            };
        }
        /// <summary>
        /// Parses out media UDIs from Macro Grid Control Parameters
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromGridControlMacros(IEnumerable<GridValue.GridControl> macroGridControls)
        {
            List<Tuple<string, Dictionary<string, string>>> foundMacros = new List<Tuple<string, Dictionary<string, string>>>();

            foreach (var macroGridControl in macroGridControls)
            {
                //deserialise JSON of Macro Grid Control to a class 
                var gridMacro = macroGridControl.Value.ToObject<GridMacro>();
                //collect any macro parameters that contain the media udi format
                if (gridMacro != null && gridMacro.MacroParameters != null && gridMacro.MacroParameters.Any())
                {
                    foundMacros.Add(new Tuple<string, Dictionary<string, string>>(gridMacro.MacroAlias, gridMacro.MacroParameters));
                }
            }
            foreach (var umbracoEntityReference in GetUmbracoEntityReferencesFromMacros(foundMacros))
            {
                yield return umbracoEntityReference;
            };
        }
        // poco class to deserialise the Json for a Macro Control
        private class GridMacro
        {
            [JsonProperty("macroAlias")]
            public string MacroAlias { get; set; }
            [JsonProperty("macroParamsDictionary")]
            public Dictionary<string, string> MacroParameters { get; set; }
        }
    }
}
