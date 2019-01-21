using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.PropertyEditors;
using File = System.IO.File;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Common helper properties and methods useful to testing
    /// </summary>
    public static class TestHelper
    {



        /// <summary>
        /// Gets the current assembly directory.
        /// </summary>
        /// <value>The assembly directory.</value>
        static public string CurrentAssemblyDirectory
        {
            get
            {
                var codeBase = typeof(TestHelper).Assembly.CodeBase;
                var uri = new Uri(codeBase);
                var path = uri.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Maps the given <paramref name="relativePath"/> making it rooted on <see cref="CurrentAssemblyDirectory"/>. <paramref name="relativePath"/> must start with <code>~/</code>
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static string MapPathForTest(string relativePath)
        {
            if (!relativePath.StartsWith("~/"))
                throw new ArgumentException("relativePath must start with '~/'", "relativePath");

            return relativePath.Replace("~/", CurrentAssemblyDirectory + "/");
        }

        public static void InitializeContentDirectories()
        {
            CreateDirectories(new[] { SystemDirectories.MvcViews, SystemDirectories.Media, SystemDirectories.AppPlugins });
        }

        public static void CleanContentDirectories()
        {
            CleanDirectories(new[] { SystemDirectories.MvcViews, SystemDirectories.Media });
        }

        public static void CreateDirectories(string[] directories)
        {
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                if (directoryInfo.Exists == false)
                    Directory.CreateDirectory(IOHelper.MapPath(directory));
            }
        }

        public static void CleanDirectories(string[] directories)
        {
            var preserves = new Dictionary<string, string[]>
            {
                { SystemDirectories.MvcViews, new[] {"dummy.txt"} }
            };
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
                if (directoryInfo.Exists)
                    foreach (var x in directoryInfo.GetFiles().Where(x => preserve == null || preserve.Contains(x.Name) == false))
                        x.Delete();
            }
        }

        public static void CleanUmbracoSettingsConfig()
        {
            var currDir = new DirectoryInfo(CurrentAssemblyDirectory);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile))
                File.Delete(umbracoSettingsFile);
        }

        // fixme obsolete the dateTimeFormat thing and replace with dateDelta
        public static void AssertPropertyValuesAreEqual(object actual, object expected, string dateTimeFormat = null, Func<IEnumerable, IEnumerable> sorter = null, string[] ignoreProperties = null)
        {
            const int dateDeltaMilliseconds = 500; // .5s

            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
            {
                // ignore properties that are attributed with EditorBrowsableState.Never
                var att = property.GetCustomAttribute<EditorBrowsableAttribute>(false);
                if (att != null && att.State == EditorBrowsableState.Never)
                    continue;

                // ignore explicitely ignored properties
                if (ignoreProperties != null && ignoreProperties.Contains(property.Name))
                    continue;

                var actualValue = property.GetValue(actual, null);
                var expectedValue = property.GetValue(expected, null);

                AssertAreEqual(property, expectedValue, actualValue, sorter, dateDeltaMilliseconds);
            }
        }

        private static void AssertAreEqual(PropertyInfo property, object expected, object actual, Func<IEnumerable, IEnumerable> sorter = null, int dateDeltaMilliseconds = 0)
        {
            if (!(expected is string) && expected is IEnumerable)
            {
                // sort property collection by alias, not by property ids
                // on members, built-in properties don't have ids (always zero)
                if (expected is PropertyCollection)
                    sorter = e => ((PropertyCollection) e).OrderBy(x => x.Alias);

                // compare lists
                AssertListsAreEqual(property, (IEnumerable) actual, (IEnumerable) expected, sorter, dateDeltaMilliseconds);
            }
            else if (expected is DateTime expectedDateTime)
            {
                // compare date & time with delta
                var actualDateTime = (DateTime) actual;
                var delta = (actualDateTime - expectedDateTime).TotalMilliseconds;
                Assert.IsTrue(Math.Abs(delta) <= dateDeltaMilliseconds, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expected, actual);
            }
            else if (expected is Property expectedProperty)
            {
                // compare values
                var actualProperty = (Property) actual;
                var expectedPropertyValues = expectedProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
                var actualPropertyValues = actualProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
                if (expectedPropertyValues.Length != actualPropertyValues.Length)
                    Assert.Fail($"{property.DeclaringType.Name}.{property.Name}: Expected {expectedPropertyValues.Length} but got {actualPropertyValues.Length}.");
                for (var i = 0; i < expectedPropertyValues.Length; i++)
                {
                    Assert.AreEqual(expectedPropertyValues[i].EditedValue, actualPropertyValues[i].EditedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected draft value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
                    Assert.AreEqual(expectedPropertyValues[i].PublishedValue, actualPropertyValues[i].PublishedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected published value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
                }
            }
            else if (expected is IDataEditor expectedEditor)
            {
                Assert.IsInstanceOf<IDataEditor>(actual);
                var actualEditor = (IDataEditor) actual;
                Assert.AreEqual(expectedEditor.Alias,  actualEditor.Alias);
                // what else shall we test?
            }
            else
            {
                // directly compare values
                Assert.AreEqual(expected, actual, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name,
                    expected?.ToString() ?? "<null>", actual?.ToString() ?? "<null>");
            }
        }

        private static void AssertListsAreEqual(PropertyInfo property, IEnumerable expected, IEnumerable actual, Func<IEnumerable, IEnumerable> sorter = null, int dateDeltaMilliseconds = 0)
        {


            if (sorter == null)
            {
                // this is pretty hackerific but saves us some code to write
                sorter = enumerable =>
                {
                    // semi-generic way of ensuring any collection of IEntity are sorted by Ids for comparison
                    var entities = enumerable.OfType<IEntity>().ToList();
                    return entities.Count > 0 ? (IEnumerable) entities.OrderBy(x => x.Id) : entities;
                };
            }

            var expectedListEx = sorter(expected).Cast<object>().ToList();
            var actualListEx = sorter(actual).Cast<object>().ToList();

            if (actualListEx.Count != expectedListEx.Count)
                Assert.Fail("Collection {0}.{1} does not match. Expected IEnumerable containing {2} elements but was IEnumerable containing {3} elements", property.PropertyType.Name, property.Name, expectedListEx.Count, actualListEx.Count);

            for (var i = 0; i < actualListEx.Count; i++)
                AssertAreEqual(property, expectedListEx[i], actualListEx[i], sorter, dateDeltaMilliseconds);
        }

        public static void DeleteDirectory(string path)
        {
            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    File.Delete(file);
            });

            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                Directory.Delete(path, true);
            });
        }

        public static void TryAssert(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<AssertionException>(action, maxTries, waitMilliseconds);
        }

        public static void Try(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<Exception>(action, maxTries, waitMilliseconds);
        }

        public static void Try<T>(Action action, int maxTries = 5, int waitMilliseconds = 200)
            where T : Exception
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (T)
                {
                    if (tries++ > maxTries)
                        throw;
                    Thread.Sleep(waitMilliseconds);
                }
            }
        }
    }
}
