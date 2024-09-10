using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeValidationEditingBuilder
    : ChildBuilderBase<PropertyTypeEditingBuilder, PropertyTypeValidation>, IBuildPropertyTypes, IWithMandatoryBuilder,
        IWithMandatoryMessageBuilder, IWithRegularExpressionBuilder, IWithRegularExpressionMessage
{
    private bool? _mandatory;
    private string? _mandatoryMessage;
    private string? _regularExpression;
    private string? _regularExpressionMessage;

    public PropertyTypeValidationEditingBuilder(PropertyTypeEditingBuilder parentBuilder) : base(parentBuilder)
    {
    }

    bool? IWithMandatoryBuilder.Mandatory
    {
        get => _mandatory;
        set => _mandatory = value;
    }

    string? IWithMandatoryMessageBuilder.MandatoryMessage
    {
        get => _mandatoryMessage;
        set => _mandatoryMessage = value;
    }

    string? IWithRegularExpressionBuilder.RegularExpression
    {
        get => _regularExpression;
        set => _regularExpression = value;
    }

    string? IWithRegularExpressionMessage.RegularExpressionMessage
    {
        get => _regularExpressionMessage;
        set => _regularExpressionMessage = value;
    }

    public override PropertyTypeValidation Build()
    {
        var validation = new PropertyTypeValidation
        {
            Mandatory = _mandatory ?? false,
            MandatoryMessage = _mandatoryMessage ?? string.Empty,
            RegularExpression = _regularExpression ?? string.Empty,
            RegularExpressionMessage = _regularExpressionMessage ?? string.Empty,
        };

        return validation;
    }
}
