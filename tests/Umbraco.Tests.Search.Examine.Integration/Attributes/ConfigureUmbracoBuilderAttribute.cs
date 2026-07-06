namespace Umbraco.Tests.Search.Examine.Integration.Attributes;

// the core ConfigureBuilderAttribute won't execute from other assemblies at the moment, so this is a workaround
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ConfigureUmbracoBuilderAttribute : Attribute
{
    public required string ActionName { get; init; }

    public void Execute(IUmbracoBuilder builder, Type testType)
        => testType.GetMethods().FirstOrDefault(method => method.Name == ActionName)?.Invoke(null, [builder]);
}
