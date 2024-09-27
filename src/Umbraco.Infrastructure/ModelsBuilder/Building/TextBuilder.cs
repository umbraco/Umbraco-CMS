using System.Text;
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
    /// <param name="builderBase"></param>
    /// <param name="textBuilderActions"></param>
    public TextBuilder(IBuilderBase builderBase, ITextBuilderActions textBuilderActions)
    {
        _builderBase = builderBase;
        _textBuilderActions = textBuilderActions;
    }

    /// <inheritdoc/>
    public void Generate(StringBuilder sb, TypeModel typeModel, IEnumerable<TypeModel> availableTypes) => Generate(sb, [typeModel], availableTypes, false);

    /// <inheritdoc/>
    public void Generate(StringBuilder sb, IEnumerable<TypeModel> typeModels, IEnumerable<TypeModel> availableTypes, bool addAssemblyMarker)
    {
        _builderBase.Prepare(availableTypes);

        _textBuilderActions.WriteHeader(sb);

        _textBuilderActions.WriteUsing(sb, _builderBase.Using);

        // assembly attributes marker
        if (addAssemblyMarker)
        {
            _textBuilderActions.WriteAssemblyAttributesMarker(sb);
        }

        _textBuilderActions.WriteNamespace(sb, _builderBase.GetModelsNamespace());

        foreach (TypeModel typeModel in typeModels)
        {
            _textBuilderActions.WriteContentType(sb, typeModel, true);
            _textBuilderActions.WriteContentTypeProperties(sb, typeModel);
            _textBuilderActions.WriteCustomCodeBeforeClassClose(sb, typeModel);
            CloseClass(sb);
        }

        CloseNameSpace(sb);
    }

    private void CloseClass(StringBuilder sb) => sb.Append("\t}\n");

    private void CloseNameSpace(StringBuilder sb) => sb.Append("}\n");

}
