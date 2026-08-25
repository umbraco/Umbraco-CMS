using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

// Schema lockdown is enforced as an authorization requirement, and authorization is skipped outright for anything
// carrying IAllowAnonymous. A governed controller that also allowed anonymous access would therefore look governed
// and be silently exempt, so this reads the Management API assembly and fails on any such pairing.
[TestFixture]
public class GovernedControllerAnonymousAccessTests
{
    private static readonly Type[] GovernedControllers =
        typeof(SchemaEntityTypeAttribute).Assembly
            .GetTypes()
            .Where(type => type.GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true) is not null)
            .ToArray();

    // The scan reads the Management API assembly, so these exercise the detector on both declaration sites without
    // being able to reach the scan themselves.
    [AllowAnonymous]
    private class ClassLevelAnonymousController
    {
        public void Post()
        {
        }
    }

    private class ActionLevelAnonymousController
    {
        [AllowAnonymous]
        public void Post()
        {
        }
    }

    [Test]
    public void No_Governed_Controller_Allows_Anonymous_Access()
    {
        Assert.That(GovernedControllers, Is.Not.Empty);

        var exempt = GovernedControllers
            .SelectMany(DescribeAnonymousAccess)
            .ToArray();

        Assert.That(
            exempt,
            Is.Empty,
            $"Governed controllers allowing anonymous access: {string.Join(", ", exempt)}.");
    }

    [TestCase(typeof(ClassLevelAnonymousController))]
    [TestCase(typeof(ActionLevelAnonymousController))]
    public void Anonymous_Access_Is_Detected(Type controller)
        => Assert.That(DescribeAnonymousAccess(controller), Is.Not.Empty);

    private static IEnumerable<string> DescribeAnonymousAccess(Type controller)
    {
        if (AllowsAnonymous(controller.GetCustomAttributes(inherit: true)))
        {
            yield return controller.FullName!;
        }

        foreach (MethodInfo method in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                     .Where(method => method.DeclaringType != typeof(object)))
        {
            if (AllowsAnonymous(method.GetCustomAttributes(inherit: true)))
            {
                yield return $"{controller.FullName}.{method.Name}";
            }
        }
    }

    // AllowAnonymousAttribute is only one way to carry the metadata the authorization middleware looks for, so the
    // interface is what has to be checked.
    private static bool AllowsAnonymous(object[] attributes) => attributes.OfType<IAllowAnonymous>().Any();
}
