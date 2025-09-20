using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Templates;

public sealed class HtmlMacroParameterParser : IHtmlMacroParameterParser
{
    private readonly ILogger<HtmlMacroParameterParser> _logger;
    private readonly IMacroService _macroService;
    private readonly ParameterEditorCollection _parameterEditors;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactories;

    [Obsolete("Use the non-obsolete overload instead, scheduled for removal in v14")]
    public HtmlMacroParameterParser(IMacroService macroService, ILogger<HtmlMacroParameterParser> logger, ParameterEditorCollection parameterEditors)
        : this(
            macroService,
            logger,
            parameterEditors,
            StaticServiceProvider.Instance.GetRequiredService<DataValueReferenceFactoryCollection>())
    { }

    public HtmlMacroParameterParser(IMacroService macroService, ILogger<HtmlMacroParameterParser> logger, ParameterEditorCollection parameterEditors, DataValueReferenceFactoryCollection dataValueReferenceFactories)
    {
        _macroService = macroService;
        _logger = logger;
        _parameterEditors = parameterEditors;
        _dataValueReferenceFactories = dataValueReferenceFactories;
    }

    /// <summary>
    ///     Parses out media UDIs from an HTML string based on embedded macro parameter values.
    /// </summary>
    /// <param name="text">HTML string</param>
    /// <returns></returns>
    public IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromEmbeddedMacros(string text)
    {
        // There may be more than one macro with the same alias on the page so using a tuple
        var foundMacros = new List<Tuple<string?, Dictionary<string, string>>>();

        // This legacy ParseMacros() already finds the macros within a Rich Text Editor using regexes
        // It seems to lowercase the macro parameter alias - so making the dictionary case insensitive
        MacroTagParser.ParseMacros(
            text,
            textblock => { },
            (macroAlias, macroAttributes) => foundMacros.Add(new Tuple<string?, Dictionary<string, string>>(
                macroAlias,
                new Dictionary<string, string>(macroAttributes, StringComparer.OrdinalIgnoreCase))));

        return foundMacros.Count > 0
            ? GetUmbracoEntityReferencesFromMacros(foundMacros)
            : Enumerable.Empty<UmbracoEntityReference>();
    }

    /// <summary>
    ///     Parses out media UDIs from Macro Grid Control parameters.
    /// </summary>
    /// <param name="macroGridControls"></param>
    /// <returns></returns>
    public IEnumerable<UmbracoEntityReference> FindUmbracoEntityReferencesFromGridControlMacros(IEnumerable<GridValue.GridControl> macroGridControls)
    {
        var foundMacros = new List<Tuple<string?, Dictionary<string, string>>>();

        foreach (GridValue.GridControl macroGridControl in macroGridControls)
        {
            // Deserialise JSON of Macro Grid Control to a class
            GridMacro? gridMacro = macroGridControl.Value?.ToObject<GridMacro>();

            // Collect any macro parameters that contain the media udi format
            if (gridMacro is not null && gridMacro.MacroParameters is not null && gridMacro.MacroParameters.Any())
            {
                foundMacros.Add(new Tuple<string?, Dictionary<string, string>>(gridMacro.MacroAlias, gridMacro.MacroParameters));
            }
        }

        return foundMacros.Count > 0
            ? GetUmbracoEntityReferencesFromMacros(foundMacros)
            : Enumerable.Empty<UmbracoEntityReference>();
    }

    private IEnumerable<UmbracoEntityReference> GetUmbracoEntityReferencesFromMacros(
        List<Tuple<string?, Dictionary<string, string>>> macros)
    {
        if (_macroService is not IMacroService macroWithAliasService)
        {
            yield break;
        }


        IEnumerable<string?> uniqueMacroAliases = macros.Select(f => f.Item1).Distinct();

        // TODO: Tracking Macro references
        // Here we are finding the used macros' Udis (there should be a Related Macro relation type - but Relations don't accept 'Macro' as an option)
        var foundMacroUmbracoEntityReferences = new List<UmbracoEntityReference>();

        // Get all the macro configs in one hit for these unique macro aliases - this is now cached with a custom cache policy
        IEnumerable<IMacro> macroConfigs = uniqueMacroAliases.Any()
            ? macroWithAliasService.GetAll(uniqueMacroAliases.WhereNotNull().ToArray())
            : Enumerable.Empty<IMacro>();

        foreach (Tuple<string?, Dictionary<string, string>> macro in macros)
        {
            IMacro? macroConfig = macroConfigs.FirstOrDefault(f => f.Alias == macro.Item1);
            if (macroConfig is null)
            {
                continue;
            }

            foundMacroUmbracoEntityReferences.Add(new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Macro, macroConfig.Key)));

            // Only do this if the macros actually have parameters
            if (macroConfig.Properties.Keys.Any(f => f != "macroAlias"))
            {
                foreach (UmbracoEntityReference umbracoEntityReference in GetUmbracoEntityReferencesFromMacroParameters(macro.Item2, macroConfig, _parameterEditors))
                {
                    yield return umbracoEntityReference;
                }
            }
        }
    }

    /// <summary>
    ///     Finds media UDIs in Macro Parameter Values by calling the GetReference method for all the Macro Parameter Editors
    ///     for a particular macro.
    /// </summary>
    /// <param name="macroParameters">The parameters for the macro a dictionary of key/value strings</param>
    /// <param name="macroConfig">
    ///     The macro configuration for this particular macro - contains the types of editors used for
    ///     each parameter
    /// </param>
    /// <param name="parameterEditors">
    ///     A list of all the registered parameter editors used in the Umbraco implmentation - to
    ///     look up the corresponding property editor for a macro parameter
    /// </param>
    /// <returns></returns>
    private IEnumerable<UmbracoEntityReference> GetUmbracoEntityReferencesFromMacroParameters(Dictionary<string, string> macroParameters, IMacro macroConfig, ParameterEditorCollection parameterEditors)
    {
        foreach (IMacroProperty parameter in macroConfig.Properties)
        {
            if (macroParameters.TryGetValue(parameter.Alias, out var parameterValue))
            {
                var parameterEditorAlias = parameter.EditorAlias;
                IDataEditor? parameterEditor = parameterEditors.FirstOrDefault(f => string.Equals(f.Alias, parameterEditorAlias, StringComparison.OrdinalIgnoreCase));
                if (parameterEditor is not null)
                {
                    foreach (UmbracoEntityReference entityReference in _dataValueReferenceFactories.GetReferences(parameterEditor, parameterValue))
                    {
                        yield return entityReference;
                    }
                }
            }
        }
    }

    // Poco class to deserialise the Json for a Macro Control
    private class GridMacro
    {
        [JsonProperty("macroAlias")]
        public string? MacroAlias { get; set; }

        [JsonProperty("macroParamsDictionary")]
        public Dictionary<string, string>? MacroParameters { get; set; }
    }
}
