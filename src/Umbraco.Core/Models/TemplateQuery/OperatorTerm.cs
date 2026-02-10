namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents an operator term that defines a comparison operation and the types it applies to.
/// </summary>
public class OperatorTerm
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OperatorTerm" /> class with default values.
    /// </summary>
    public OperatorTerm()
    {
        Name = "is";
        Operator = Operator.Equals;
        AppliesTo = new[] { "string" };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperatorTerm" /> class with specified values.
    /// </summary>
    /// <param name="name">The display name of the operator.</param>
    /// <param name="operator">The operator enumeration value.</param>
    /// <param name="appliesTo">The collection of property types this operator applies to.</param>
    public OperatorTerm(string name, Operator @operator, IEnumerable<string> appliesTo)
    {
        Name = name;
        Operator = @operator;
        AppliesTo = appliesTo;
    }

    /// <summary>
    ///     Gets or sets the display name of the operator.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the operator enumeration value.
    /// </summary>
    public Operator Operator { get; set; }

    /// <summary>
    ///     Gets or sets the collection of property types this operator applies to.
    /// </summary>
    public IEnumerable<string> AppliesTo { get; set; }
}
