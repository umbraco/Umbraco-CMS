using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeValidationEditingBuilder<TParent>
    : ChildBuilderBase<PropertyTypeEditingBuilder<TParent>, PropertyTypeValidation>,
        IBuildPropertyTypes,
        IWithMandatoryBuilder,
        IWithMandatoryMessageBuilder,
        IWithRegularExpressionBuilder,
        IWithRegularExpressionMessage
{
    private bool? _mandatory;
    private string? _mandatoryMessage;
    private string? _regularExpression;
    private string? _regularExpressionMessage;

    public PropertyTypeValidationEditingBuilder(PropertyTypeEditingBuilder<TParent> parentBuilder)
        : base(parentBuilder)
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
            MandatoryMessage = _mandatoryMessage ?? null,
            RegularExpression = _regularExpression ?? null,
            RegularExpressionMessage = _regularExpressionMessage ?? null,
        };

        return validation;
    }
}
