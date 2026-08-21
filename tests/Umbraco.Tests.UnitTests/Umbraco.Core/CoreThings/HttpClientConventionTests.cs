// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.NewsDashboard;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

[TestFixture]
public class HttpClientConventionTests
{
    private static readonly Assembly[] Assemblies =
    [
        typeof(Constants).Assembly,
        typeof(NewsDashboardService).Assembly,
    ];

    [Test]
    public void Cannot_Declare_Mutable_Static_HttpClient_Field()
    {
        var offenders = Assemblies
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
            "Static HttpClient fields must be readonly and fully configured on assignment. Use SharedHttpClient "
            + "for existing call sites that cannot take a dependency, or inject IHttpClientFactory in new code.");
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
