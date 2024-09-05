using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeValidationEditingBuilder<TParent>
    : ChildBuilderBase<TParent, PropertyTypeValidation>, IWithMandatoryBuilder,
        IWithMandatoryMessageBuilder, IWithRegularExpressionBuilder, IWithRegularExpressionMessage
{
    private bool _mandatory;
    private string? _mandatoryMessage;
    private string? _regularExpression;
    private string? _regularExpressionMessage;

    public PropertyTypeValidationEditingBuilder(TParent parentBuilder) : base(parentBuilder)
    {
    }

    public bool Mandatory
    {
        get => _mandatory;
        set => _mandatory = value;
    }

    public string? MandatoryMessage
    {
        get => _mandatoryMessage;
        set => _mandatoryMessage = value;
    }

    public string? RegularExpression
    {
        get => _regularExpression;
        set => _regularExpression = value;
    }

    public string? RegularExpressionMessage
    {
        get => _regularExpressionMessage;
        set => _regularExpressionMessage = value;
    }

    public override PropertyTypeValidation Build()
    {
        var validation = new PropertyTypeValidation
        {
            Mandatory = _mandatory,
            MandatoryMessage = _mandatoryMessage,
            RegularExpression = _regularExpression,
            RegularExpressionMessage = _regularExpressionMessage,
        };

        return validation;
    }
}

