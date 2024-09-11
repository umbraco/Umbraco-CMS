using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

/// <summary>
///     Implements a builder that works by writing text.
/// </summary>
public class TextBuilder : ITextBuilder
{
    private readonly IBuilderBase _builderBase;
    private readonly ITextBuilderActions _textBuilderActions;
    /// <summary>
    ///     Initializes a new instance of the <see cref="TextBuilder" /> class with a list of models to generate
    ///     and the result of code parsing.
    /// </summary>
    /// <param name="typeModels">The list of models to generate.</param>
    /// <param name="builderBase"></param>
    public TextBuilder(IBuilderBase builderBase, ITextBuilderActions textBuilderActions)
    {
        _builderBase = builderBase;
        _textBuilderActions = textBuilderActions;
    }

    /// <summary>
    ///     Outputs an "auto-generated" header to a string builder.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    public static void WriteHeader(StringBuilder sb) => TextHeaderWriter.WriteHeader(sb);

    /// <summary>
    ///     Outputs a generated model to a string builder.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="typeModel">The model to generate.</param>
    public void Generate(StringBuilder sb, TypeModel typeModel)
    {

        _textBuilderActions.WriteHeader(sb);

        _textBuilderActions.WriteUsing(sb, _builderBase.Using);

        _textBuilderActions.WriteNamespace(sb, _builderBase.GetModelsNamespace());

        _textBuilderActions.WriteContentType(sb, typeModel, false);
        _textBuilderActions.WriteContentTypeProperties(sb, typeModel);

        _textBuilderActions.WriteCloseClass(sb);
    }

    /// <summary>
    ///     Outputs generated models to a string builder.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    /// <param name="typeModels">The models to generate.</param>
    public void Generate(StringBuilder sb, IEnumerable<TypeModel> typeModels)
    {
        _textBuilderActions.WriteHeader(sb);

        _textBuilderActions.WriteUsing(sb, _builderBase.Using);

        // assembly attributes marker
        _textBuilderActions.WriteAssemblyAttributesMarker(sb);

        _textBuilderActions.WriteNamespace(sb, _builderBase.GetModelsNamespace());

        foreach (TypeModel typeModel in typeModels)
        {
            _textBuilderActions.WriteContentType(sb, typeModel, true);
            _textBuilderActions.WriteContentTypeProperties(sb, typeModel);
        }

        _textBuilderActions.WriteCloseClass(sb);
    }

}
