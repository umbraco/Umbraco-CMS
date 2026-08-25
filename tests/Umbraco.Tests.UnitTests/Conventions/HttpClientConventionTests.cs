// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Conventions;

[TestFixture]
public class HttpClientConventionTests
{
    [Test]
    public void Cannot_Declare_Mutable_Static_HttpClient_Field()
    {
        Assembly[] assemblies = GetUmbracoAssemblies();

        // Guard against the discovery silently covering nothing if the output layout ever changes.
        Assert.That(assemblies, Is.Not.Empty, "No Umbraco assemblies were discovered to check.");

        var offenders = assemblies
            .SelectMany(GetLoadableTypes)
            .SelectMany(type => type.GetFields(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(field => field.FieldType == typeof(HttpClient) && field.IsInitOnly is false)
            .Select(field => $"{field.DeclaringType?.FullName}.{field.Name}")
            .OrderBy(name => name)
            .ToArray();

        // A mutable static field is assigned lazily and then configured, so another thread can observe the
        // client before its default request headers are complete. HttpHeaders is not thread safe, so the
        // in-flight request faults while writing them (#23697).
        Assert.That(
            offenders,
            Is.Empty,
            "Static HttpClient fields must be readonly and fully configured on assignment. Use SharedHttpClient for existing call sites that cannot take a dependency, or inject IHttpClientFactory in new code.");
    }

    private static Assembly[] GetUmbracoAssemblies()
        => Directory
            .EnumerateFiles(AppContext.BaseDirectory, "Umbraco.*.dll")
            .Where(path => Path.GetFileName(path).StartsWith("Umbraco.Tests.", StringComparison.Ordinal) is false)
            .Select(TryLoad)
            .Where(assembly => assembly is not null)
            .ToArray()!;

    private static Assembly? TryLoad(string path)
    {
        try
        {
            return Assembly.LoadFrom(path);
        }
        catch (Exception exception) when (exception is BadImageFormatException or FileLoadException)
        {
            return null;
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }
}
