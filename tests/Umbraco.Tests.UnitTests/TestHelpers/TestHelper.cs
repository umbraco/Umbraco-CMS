// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using File = System.IO.File;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

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

    /// <summary>
    /// Gets the scope provider used for managing scopes in tests.
    /// </summary>
    public static IScopeProvider ScopeProvider => s_testHelperInternal.ScopeProvider;
    /// <summary>
    /// Gets the core scope provider used for unit testing.
    /// </summary>
    public static ICoreScopeProvider CoreScopeProvider => s_testHelperInternal.ScopeProvider;
    /// <summary>
    /// Gets the static instance of <see cref="IShortStringHelper"/> used for testing purposes.
    /// This helper provides methods for manipulating and formatting short strings in unit tests.
    /// </summary>
    public static IShortStringHelper ShortStringHelper => s_testHelperInternal.ShortStringHelper;

    /// <summary>
    /// Gets the <see cref="IJsonSerializer"/> instance used internally by the test helper for JSON serialization operations.
    /// </summary>
    public static IJsonSerializer JsonSerializer => s_testHelperInternal.JsonSerializer;

    /// <summary>
    /// Gets the <see cref="IVariationContextAccessor"/> instance used for managing variation contexts during unit tests.
    /// </summary>
    public static IVariationContextAccessor VariationContextAccessor => s_testHelperInternal.VariationContextAccessor;

    /// <summary>
    /// Gets the bulk SQL insert provider used for performing bulk insert operations in tests.
    /// </summary>
    public static IBulkSqlInsertProvider BulkSqlInsertProvider => s_testHelperInternal.BulkSqlInsertProvider;

    /// <summary>
    /// Gets the <see cref="IMarchal"/> instance used internally by the test helper for testing purposes.
    /// </summary>
    public static IMarchal Marchal => s_testHelperInternal.Marchal;

    /// <summary>
    /// Gets the current <see cref="CoreDebugSettings"/> instance used by the test helper.
    /// </summary>
    public static CoreDebugSettings CoreDebugSettings => s_testHelperInternal.CoreDebugSettings;

    /// <summary>
    /// Gets the <see cref="IIOHelper"/> instance used for testing purposes.
    /// </summary>
    public static IIOHelper IOHelper => s_testHelperInternal.IOHelper;

    /// <summary>
    /// Gets the main domain instance used for testing.
    /// </summary>
    public static IMainDom MainDom => s_testHelperInternal.MainDom;

    /// <summary>
    /// Gets the static <see cref="UriUtility"/> instance used for testing purposes.
    /// </summary>
    public static UriUtility UriUtility => s_testHelperInternal.UriUtility;

    /// <summary>
    /// Gets a static instance of <see cref="IEmailSender"/> for testing purposes.
    /// </summary>
    public static IEmailSender EmailSender { get; } = new EmailSender(new NullLogger<EmailSender>(), new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()), Mock.Of<IEventAggregator>(), Mock.Of<IEmailSenderClient>(), null,null);

    /// <summary>
    /// Retrieves an <see cref="ITypeFinder"/> instance used for locating types during unit tests.
    /// </summary>
    /// <returns>An <see cref="ITypeFinder"/> instance for use in test scenarios.</returns>
    public static ITypeFinder GetTypeFinder() => s_testHelperInternal.GetTypeFinder();

    /// <summary>
    /// Gets a mocked instance of the <see cref="TypeLoader"/>.
    /// </summary>
    /// <returns>A mocked <see cref="TypeLoader"/> instance.</returns>
    public static TypeLoader GetMockedTypeLoader() => s_testHelperInternal.GetMockedTypeLoader();

    /// <summary>
    /// Gets a lazy-initialized mock instance of <see cref="ISqlContext"/>.
    /// </summary>
    /// <returns>A lazy instance of a mocked <see cref="ISqlContext"/>.</returns>
    public static Lazy<ISqlContext> GetMockSqlContext()
    {
        var sqlContext = Mock.Of<ISqlContext>();
        var syntax = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
        Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(syntax);
        return new Lazy<ISqlContext>(() => sqlContext);
    }

    /// <summary>
    /// Creates and returns a new instance of <see cref="MapperConfigurationStore"/>.
    /// </summary>
    /// <returns>A new <see cref="MapperConfigurationStore"/> instance.</returns>
    public static MapperConfigurationStore CreateMaps() => new();

/// <summary>
///     Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based
///     on a virtual path name
/// </summary>
/// <param name="relativePath">The relative virtual path to the test file.</param>
/// <returns>The physical file path corresponding to the given relative path.</returns>
    public static string MapPathForTestFiles(string relativePath) =>
        s_testHelperInternal.MapPathForTestFiles(relativePath);

    /// <summary>
    /// Initializes the content directories required by the application.
    /// </summary>
    public static void InitializeContentDirectories() => CreateDirectories(new[]
    {
        Constants.SystemDirectories.MvcViews,
        new GlobalSettings().UmbracoMediaPhysicalRootPath,
        Constants.SystemDirectories.AppPlugins,
    });

    /// <summary>
    /// Cleans the content directories used by the system.
    /// </summary>
    public static void CleanContentDirectories() => CleanDirectories(new[]
    {
        Constants.SystemDirectories.MvcViews,
        new GlobalSettings().UmbracoMediaPhysicalRootPath,
    });

    /// <summary>
    /// Creates the specified directories if they do not already exist.
    /// </summary>
    /// <param name="directories">An array of directory paths to create.</param>
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

    /// <summary>
    /// Deletes all files in the specified directories, except for files that are explicitly preserved by internal rules.
    /// </summary>
    /// <param name="directories">An array of directory paths to clean.</param>
    /// <remarks>
    /// Currently, only certain files (e.g., "dummy.txt" in the MVC views directory) are preserved; all other files are deleted.
    /// </remarks>
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

    /// <summary>
    /// Deletes the umbracoSettings.config file from the config directory if it exists.
    /// </summary>
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
    // TODO: obsolete the dateTimeFormat thing and replace with dateDelta
    /// <summary>
    /// Asserts that the public property values of two objects are equal, with options to customize comparison behavior.
    /// </summary>
    /// <param name="actual">The object whose property values are being tested.</param>
    /// <param name="expected">The object providing the expected property values.</param>
    /// <param name="dateTimeFormat">An optional date time format string used for comparing date properties. (Obsolete: comparison uses a fixed delta internally.)</param>
    /// <param name="sorter">An optional function to sort <see cref="IEnumerable"/> properties before comparison, to ensure order-independent equality.</param>
    /// <param name="ignoreProperties">An optional array of property names to ignore during comparison.</param>
    /// <remarks>
    /// Properties marked with <see cref="EditorBrowsableState.Never"/> or listed in <paramref name="ignoreProperties"/> are skipped. Date properties are compared with a tolerance. Throws an assertion exception if any property values differ.
    /// </remarks>
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

    /// <summary>
    /// Gets the current Umbraco version.
    /// </summary>
    /// <returns>The current Umbraco version.</returns>
    public static IUmbracoVersion GetUmbracoVersion() => s_testHelperInternal.GetUmbracoVersion();

    /// <summary>
    /// Creates and returns a new IServiceCollection with lazy support added.
    /// </summary>
    /// <returns>A new IServiceCollection instance with lazy support.</returns>
    public static IServiceCollection GetServiceCollection() => new ServiceCollection().AddLazySupport();

    /// <summary>
    /// Retrieves the <see cref="IHostingEnvironment"/> instance used for testing purposes.
    /// </summary>
    /// <returns>The <see cref="IHostingEnvironment"/> used by the test helper.</returns>
    public static IHostingEnvironment GetHostingEnvironment() => s_testHelperInternal.GetHostingEnvironment();

    /// <summary>
    /// Gets the logging configuration based on the provided hosting environment.
    /// </summary>
    /// <param name="hostingEnv">The hosting environment.</param>
    /// <returns>The logging configuration.</returns>
    public static ILoggingConfiguration GetLoggingConfiguration(IHostingEnvironment hostingEnv) =>
        s_testHelperInternal.GetLoggingConfiguration(hostingEnv);

    /// <summary>Gets the hosting environment lifetime.</summary>
    /// <returns>An instance of <see cref="IApplicationShutdownRegistry"/> representing the hosting environment lifetime.</returns>
    public static IApplicationShutdownRegistry GetHostingEnvironmentLifetime() =>
        s_testHelperInternal.GetHostingEnvironmentLifetime();

    /// <summary>Gets an instance of <see cref="IIpResolver"/> for testing purposes.</summary>
    /// <returns>An <see cref="IIpResolver"/> instance.</returns>
    public static IIpResolver GetIpResolver() => s_testHelperInternal.GetIpResolver();

    /// <summary>
    /// Retrieves the current <see cref="IRequestCache"/> instance used for request caching in tests.
    /// </summary>
    /// <returns>The <see cref="IRequestCache"/> instance associated with the current test context.</returns>
    public static IRequestCache GetRequestCache() => s_testHelperInternal.GetRequestCache();

    /// <summary>
    /// Retrieves an <see cref="IPublishedUrlProvider"/> instance for use in unit tests.
    /// </summary>
    /// <returns>An <see cref="IPublishedUrlProvider"/> used to generate published URLs in test scenarios.</returns>
    public static IPublishedUrlProvider GetPublishedUrlProvider() => s_testHelperInternal.GetPublishedUrlProvider();

    private class TestHelperInternal : TestHelperBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestHelperInternal"/> class.
    /// </summary>
        public TestHelperInternal()
            : base(typeof(TestHelperInternal).Assembly)
        {
        }

    /// <summary>
    /// Gets the mocked bulk SQL insert provider used for testing purposes.
    /// </summary>
        public override IBulkSqlInsertProvider BulkSqlInsertProvider { get; } = Mock.Of<IBulkSqlInsertProvider>();

    /// <summary>
    /// Gets the Marchal instance.
    /// </summary>
        public override IMarchal Marchal { get; } = Mock.Of<IMarchal>();

    /// <summary>
    /// Gets the hosting environment for the test context.
    /// </summary>
    /// <returns>An <see cref="IHostingEnvironment"/> instance representing the test hosting environment.</returns>
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

    /// <summary>
    /// Gets the hosting environment lifetime.
    /// </summary>
    /// <returns>An instance of <see cref="IApplicationShutdownRegistry"/>.</returns>
        public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime()
            => Mock.Of<IApplicationShutdownRegistry>();

    /// <summary>
    /// Gets an IP resolver instance.
    /// </summary>
    /// <returns>An instance of <see cref="IIpResolver"/>.</returns>
        public override IIpResolver GetIpResolver()
            => Mock.Of<IIpResolver>();
    }
}
