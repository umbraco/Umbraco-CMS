using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ConfigureBuilderAttribute : Attribute
{
    public string ActionName { get; set; }

    public void Execute(IUmbracoBuilder builder)
    {
        // todo allow to find methods from parents
        Type.GetType(TestContext.CurrentContext.Test.ClassName).GetMethods().First(method => method.Name == ActionName)
            .Invoke(null, [builder]);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ConfigureBuilderTestCaseAttribute : Attribute
{
    public string ActionName { get; set; }

    public int IndexOfParameter { get; set; }

    public void Execute(IUmbracoBuilder builder)
    {
        Type.GetType(TestContext.CurrentContext.Test.ClassName).GetMethods().First(method => method.Name == ActionName)
            .Invoke(null, [builder, TestContext.CurrentContext.Test.Arguments[IndexOfParameter]]);
    }
}
