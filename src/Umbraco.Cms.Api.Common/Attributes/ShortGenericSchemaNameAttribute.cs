namespace Umbraco.Cms.Api.Common.Attributes;

public abstract class ShortGenericSchemaNameAttribute : Attribute
{
    public Type[] GenericTypes { get; set; }

    public string SchemaName { get; set; }

    public ShortGenericSchemaNameAttribute(string schemaName, Type[] genericTypes)
    {
        GenericTypes = genericTypes;
        SchemaName = schemaName;
    }
}

public class ShortGenericSchemaNameAttribute<T1, T2> : ShortGenericSchemaNameAttribute
{
    public ShortGenericSchemaNameAttribute(string schemaName)
        : base(schemaName, new[] { typeof(T1), typeof(T2) })
    {
    }
}
