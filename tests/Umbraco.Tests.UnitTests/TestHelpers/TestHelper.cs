// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using File = System.IO.File;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers;

/// <summary>
///     Common helper properties and methods useful to testing
/// </summary>
public static class TestHelper
{
    private static readonly TestHelperInternal s_testHelperInternal = new();

    /// <summary>
    ///     Gets the working directory of the test project.
    /// </summary>
    /// <value>The assembly directory.</value>
    public static string WorkingDirectory => s_testHelperInternal.WorkingDirectory;

    public static IShortStringHelper ShortStringHelper => s_testHelperInternal.ShortStringHelper;

    public static IJsonSerializer JsonSerializer => s_testHelperInternal.JsonSerializer;

    public static IVariationContextAccessor VariationContextAccessor => s_testHelperInternal.VariationContextAccessor;

    public static IBulkSqlInsertProvider BulkSqlInsertProvider => s_testHelperInternal.BulkSqlInsertProvider;

    public static IMarchal Marchal => s_testHelperInternal.Marchal;

    public static CoreDebugSettings CoreDebugSettings => s_testHelperInternal.CoreDebugSettings;

    public static IIOHelper IOHelper => s_testHelperInternal.IOHelper;

    public static IMainDom MainDom => s_testHelperInternal.MainDom;

    public static UriUtility UriUtility => s_testHelperInternal.UriUtility;

    public static IEmailSender EmailSender { get; } = new EmailSender(new NullLogger<EmailSender>(), new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()), Mock.Of<IEventAggregator>());

    public static ITypeFinder GetTypeFinder() => s_testHelperInternal.GetTypeFinder();

    public static TypeLoader GetMockedTypeLoader() => s_testHelperInternal.GetMockedTypeLoader();

    public static Lazy<ISqlContext> GetMockSqlContext()
    {
        var sqlContext = Mock.Of<ISqlContext>();
        var syntax = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
        Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(syntax);
        return new Lazy<ISqlContext>(() => sqlContext);
    }

    public static MapperConfigurationStore CreateMaps() => new();

    /// <summary>
    ///     Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based
    ///     on a virtual path name
    /// </summary>
    public static string MapPathForTestFiles(string relativePath) =>
        s_testHelperInternal.MapPathForTestFiles(relativePath);

    public static void InitializeContentDirectories() => CreateDirectories(new[]
    {
        Constants.SystemDirectories.MvcViews,
        new GlobalSettings().UmbracoMediaPhysicalRootPath,
        Constants.SystemDirectories.AppPlugins,
    });

    public static void CleanContentDirectories() => CleanDirectories(new[]
    {
        Constants.SystemDirectories.MvcViews,
        new GlobalSettings().UmbracoMediaPhysicalRootPath,
    });

    public static void CreateDirectories(string[] directories)
    {
        foreach (var directory in directories)
        {
            var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
            if (directoryInfo.Exists == false)
            {
                Directory.CreateDirectory(IOHelper.MapPath(directory));
            }
        }
    }

    public static void CleanDirectories(string[] directories)
    {
        var preserves = new Dictionary<string, string[]> { { Constants.SystemDirectories.MvcViews, new[] { "dummy.txt" } } };

        foreach (var directory in directories)
        {
            var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
            var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
            if (directoryInfo.Exists)
            {
                foreach (var fileInfo in directoryInfo.GetFiles()
                             .Where(x => preserve == null || preserve.Contains(x.Name) == false))
                {
                    fileInfo.Delete();
                }
            }
        }
    }

    public static void CleanUmbracoSettingsConfig()
    {
        var currDir = new DirectoryInfo(WorkingDirectory);

        var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
        if (File.Exists(umbracoSettingsFile))
        {
            File.Delete(umbracoSettingsFile);
        }
    }

    // TODO: Move to Assertions or AssertHelper
    // FIXME: obsolete the dateTimeFormat thing and replace with dateDelta
    public static void AssertPropertyValuesAreEqual(
        object actual,
        object expected,
        string dateTimeFormat = null,
        Func<IEnumerable, IEnumerable> sorter = null,
        string[] ignoreProperties = null)
    {
        const int dateDeltaMilliseconds = 500; // .5s

        var properties = expected.GetType().GetProperties();
        foreach (var property in properties)
        {
            // ignore properties that are attributed with EditorBrowsableState.Never
            var att = property.GetCustomAttribute<EditorBrowsableAttribute>(false);
            if (att != null && att.State == EditorBrowsableState.Never)
            {
                continue;
            }

            // ignore explicitely ignored properties
            if (ignoreProperties != null && ignoreProperties.Contains(property.Name))
            {
                continue;
            }

            var actualValue = property.GetValue(actual, null);
            var expectedValue = property.GetValue(expected, null);

            AssertAreEqual(property, expectedValue, actualValue, sorter, dateDeltaMilliseconds);
        }
    }

    private static void AssertAreEqual(PropertyInfo property, object expected, object actual, Func<IEnumerable, IEnumerable> sorter = null, int dateDeltaMilliseconds = 0)
    {
        if (!(expected is string) && expected is IEnumerable enumerable)
        {
            // sort property collection by alias, not by property ids
            // on members, built-in properties don't have ids (always zero)
            if (expected is PropertyCollection)
            {
                sorter = e => ((PropertyCollection)e).OrderBy(x => x.Alias);
            }

            // compare lists
            AssertListsAreEqual(property, (IEnumerable)actual, enumerable, sorter, dateDeltaMilliseconds);
        }
        else if (expected is DateTime expectedDateTime)
        {
            // compare date & time with delta
            var actualDateTime = (DateTime)actual;
            var delta = (actualDateTime - expectedDateTime).TotalMilliseconds;
            Assert.IsTrue(
                Math.Abs(delta) <= dateDeltaMilliseconds,
                "Property {0}.{1} does not match. Expected: {2} but was: {3}",
                property.DeclaringType.Name,
                property.Name,
                expected,
                actual);
        }
        else if (expected is Property expectedProperty)
        {
            // compare values
            var actualProperty = (Property)actual;
            var expectedPropertyValues =
                expectedProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
            var actualPropertyValues = actualProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
            if (expectedPropertyValues.Length != actualPropertyValues.Length)
            {
                Assert.Fail(
                    $"{property.DeclaringType.Name}.{property.Name}: Expected {expectedPropertyValues.Length} but got {actualPropertyValues.Length}.");
            }

            for (var i = 0; i < expectedPropertyValues.Length; i++)
            {
                Assert.AreEqual(
                    expectedPropertyValues[i].EditedValue,
                    actualPropertyValues[i].EditedValue,
                    $"{property.DeclaringType.Name}.{property.Name}: Expected draft value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
                Assert.AreEqual(
                    expectedPropertyValues[i].PublishedValue,
                    actualPropertyValues[i].PublishedValue,
                    $"{property.DeclaringType.Name}.{property.Name}: Expected published value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
            }
        }
        else if (expected is IDataEditor expectedEditor)
        {
            Assert.IsInstanceOf<IDataEditor>(actual);
            var actualEditor = (IDataEditor)actual;
            Assert.AreEqual(expectedEditor.Alias, actualEditor.Alias);

            // what else shall we test?
        }
        else
        {
            // directly compare values
            Assert.AreEqual(
                expected,
                actual,
                "Property {0}.{1} does not match. Expected: {2} but was: {3}",
                property.DeclaringType.Name,
                property.Name,
                expected?.ToString() ?? "<null>",
                actual?.ToString() ?? "<null>");
        }
    }

    private static void AssertListsAreEqual(
        PropertyInfo property,
        IEnumerable expected,
        IEnumerable actual,
        Func<IEnumerable, IEnumerable> sorter = null,
        int dateDeltaMilliseconds = 0)
    {
        if (sorter == null)
        {
            // this is pretty hackerific but saves us some code to write
            sorter = enumerable =>
            {
                // semi-generic way of ensuring any collection of IEntity are sorted by Ids for comparison
                var entities = enumerable.OfType<IEntity>().ToList();
                return entities.Count > 0 ? entities.OrderBy(x => x.Id) : entities;
            };
        }

        var expectedListEx = sorter(expected).Cast<object>().ToList();
        var actualListEx = sorter(actual).Cast<object>().ToList();

        if (actualListEx.Count != expectedListEx.Count)
        {
            Assert.Fail(
                "Collection {0}.{1} does not match. Expected IEnumerable containing {2} elements but was IEnumerable containing {3} elements",
                property.PropertyType.Name,
                property.Name,
                expectedListEx.Count,
                actualListEx.Count);
        }

        for (var i = 0; i < actualListEx.Count; i++)
        {
            AssertAreEqual(property, expectedListEx[i], actualListEx[i], sorter, dateDeltaMilliseconds);
        }
    }

    public static IUmbracoVersion GetUmbracoVersion() => s_testHelperInternal.GetUmbracoVersion();

    public static IServiceCollection GetServiceCollection() => new ServiceCollection().AddLazySupport();

    public static IHostingEnvironment GetHostingEnvironment() => s_testHelperInternal.GetHostingEnvironment();

    public static ILoggingConfiguration GetLoggingConfiguration(IHostingEnvironment hostingEnv) =>
        s_testHelperInternal.GetLoggingConfiguration(hostingEnv);

    public static IApplicationShutdownRegistry GetHostingEnvironmentLifetime() =>
        s_testHelperInternal.GetHostingEnvironmentLifetime();

    public static IIpResolver GetIpResolver() => s_testHelperInternal.GetIpResolver();

    public static IRequestCache GetRequestCache() => s_testHelperInternal.GetRequestCache();

    public static IPublishedUrlProvider GetPublishedUrlProvider() => s_testHelperInternal.GetPublishedUrlProvider();

    private class TestHelperInternal : TestHelperBase
    {
        public TestHelperInternal()
            : base(typeof(TestHelperInternal).Assembly)
        {
        }

        public override IBulkSqlInsertProvider BulkSqlInsertProvider { get; } = Mock.Of<IBulkSqlInsertProvider>();

        public override IMarchal Marchal { get; } = Mock.Of<IMarchal>();

        public override IHostingEnvironment GetHostingEnvironment()
        {
            var testPath = TestContext.CurrentContext.TestDirectory.Split("bin")[0];
            return new TestHostingEnvironment(
                Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings()),
                Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == new WebRoutingSettings()),
                Mock.Of<IWebHostEnvironment>(
                    x =>
                        x.WebRootPath == "/" &&
                        x.ContentRootPath == testPath));
        }

        public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime()
            => Mock.Of<IApplicationShutdownRegistry>();

        public override IIpResolver GetIpResolver()
            => Mock.Of<IIpResolver>();
    }
}
