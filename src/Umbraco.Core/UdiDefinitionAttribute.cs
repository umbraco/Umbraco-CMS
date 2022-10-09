namespace Umbraco.Cms.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UdiDefinitionAttribute : Attribute
{
    public UdiDefinitionAttribute(string entityType, UdiType udiType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentNullException("entityType");
        }

        if (udiType != UdiType.GuidUdi && udiType != UdiType.StringUdi)
        {
            throw new ArgumentException("Invalid value.", "udiType");
        }

        EntityType = entityType;
        UdiType = udiType;
    }

    public string EntityType { get; }

    public UdiType UdiType { get; }
}
