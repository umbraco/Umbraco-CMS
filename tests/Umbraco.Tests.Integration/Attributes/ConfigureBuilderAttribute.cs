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

    public int IndexOfParamater { get; set; }

    public void Execute(IUmbracoBuilder builder)
    {
        // todo throw better errors
        var testCaseAttributes = Type.GetType(TestContext.CurrentContext.Test.ClassName)
            .GetMethods().First(m => m.Name == TestContext.CurrentContext.Test.MethodName)
            .GetCustomAttributes(typeof(NUnit.Framework.TestCaseAttribute), true).Select(attribute => attribute as NUnit.Framework.TestCaseAttribute).ToList();

        var matchingTestCaseAttribute = testCaseAttributes
            .FirstOrDefault(attribute => attribute.Arguments.Length == TestContext.CurrentContext.Test.Arguments.Length
                                         && attribute.Arguments
                                             .Select((argument, index) => TestContext.CurrentContext.Test.Arguments[index]!.Equals(argument))
                                             .All(match => match == true));

        Type.GetType(TestContext.CurrentContext.Test.ClassName).GetMethods().First(method => method.Name == ActionName)
            .Invoke(null, [builder, matchingTestCaseAttribute.Arguments[IndexOfParamater]]);
    }
}
