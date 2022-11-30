namespace Umbraco.Cms.Core.Models.TemplateQuery;

public class OperatorTerm
{
    public OperatorTerm()
    {
        Name = "is";
        Operator = Operator.Equals;
        AppliesTo = new[] { "string" };
    }

    public OperatorTerm(string name, Operator @operator, IEnumerable<string> appliesTo)
    {
        Name = name;
        Operator = @operator;
        AppliesTo = appliesTo;
    }

    public string Name { get; set; }

    public Operator Operator { get; set; }

    public IEnumerable<string> AppliesTo { get; set; }
}
