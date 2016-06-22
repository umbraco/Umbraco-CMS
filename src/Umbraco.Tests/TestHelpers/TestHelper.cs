using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.DataLayer;
using Umbraco.Core.Models.EntityBase;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
	/// Common helper properties and methods useful to testing
	/// </summary>
	public static class TestHelper
	{

		/// <summary>
		/// Clears an initialized database
		/// </summary>
		public static void ClearDatabase()
		{
            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;
			
			if (dataHelper == null)
				throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");

			dataHelper.ClearDatabase();
		}

        public static void DropForeignKeys(string table)
        {
            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;

            if (dataHelper == null)
                throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");

            dataHelper.DropForeignKeys(table);
        }

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
            CreateDirectories(new[] { SystemDirectories.Masterpages, SystemDirectories.MvcViews, SystemDirectories.Media, SystemDirectories.AppPlugins });
        }

	    public static void CleanContentDirectories()
	    {
	        CleanDirectories(new[] { SystemDirectories.Masterpages, SystemDirectories.MvcViews, SystemDirectories.Media });
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
	            { SystemDirectories.Masterpages, new[] {"dummy.txt"} },
	            { SystemDirectories.MvcViews, new[] {"dummy.txt"} }
	        };
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
                if (directoryInfo.Exists)
                    directoryInfo.GetFiles().Where(x => preserve == null || preserve.Contains(x.Name) == false).ForEach(x => x.Delete());
            }
        }

	    public static void CleanUmbracoSettingsConfig()
        {
            var currDir = new DirectoryInfo(CurrentAssemblyDirectory);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile))
                File.Delete(umbracoSettingsFile);
        }

        public static void AssertAllPropertyValuesAreEquals(object actual, object expected, string dateTimeFormat = null, Func<IEnumerable, IEnumerable> sorter = null, string[] ignoreProperties = null)
        {
            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
            {
                //ignore properties that are attributed with this
                var att = property.GetCustomAttribute<EditorBrowsableAttribute>(false);
                if (att != null && att.State == EditorBrowsableState.Never)
                    continue;

                if (ignoreProperties != null && ignoreProperties.Contains(property.Name))
                    continue;

                var expectedValue = property.GetValue(expected, null);
                var actualValue = property.GetValue(actual, null);

                if (((actualValue is string) == false) && actualValue is IEnumerable)
                {
                    AssertListsAreEquals(property, (IEnumerable)actualValue, (IEnumerable)expectedValue, dateTimeFormat, sorter);
                }
                else if (dateTimeFormat.IsNullOrWhiteSpace() == false && actualValue is DateTime)
                {
                    Assert.AreEqual(((DateTime) expectedValue).ToString(dateTimeFormat), ((DateTime)actualValue).ToString(dateTimeFormat), "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                }
                else
                {
                    Assert.AreEqual(expectedValue, actualValue, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                }
            }
        }

        private static void AssertListsAreEquals(PropertyInfo property, IEnumerable actualList, IEnumerable expectedList, string dateTimeFormat, Func<IEnumerable, IEnumerable> sorter)
        {
            if (sorter == null)
            {
                //this is pretty hackerific but saves us some code to write
                sorter = enumerable =>
                {
                    //semi-generic way of ensuring any collection of IEntity are sorted by Ids for comparison
                    var entities = enumerable.OfType<IEntity>().ToList();
                    if (entities.Count > 0)
                    {
                        return entities.OrderBy(x => x.Id);
                    }
                    else
                    {
                        return enumerable;
                    }
                };
            }          

            var actualListEx = sorter(actualList).Cast<object>().ToList();
            var expectedListEx = sorter(expectedList).Cast<object>().ToList();

            if (actualListEx.Count != expectedListEx.Count)
                Assert.Fail("Collection {0}.{1} does not match. Expected IEnumerable containing {2} elements but was IEnumerable containing {3} elements", property.PropertyType.Name, property.Name, expectedListEx.Count, actualListEx.Count);

            for (int i = 0; i < actualListEx.Count; i++)
            {
                var actualValue = actualListEx[i];
                var expectedValue = expectedListEx[i];

                if (((actualValue is string) == false) && actualValue is IEnumerable)
                {
                    AssertListsAreEquals(property, (IEnumerable)actualValue, (IEnumerable)expectedValue, dateTimeFormat, sorter);
                }
                else if (dateTimeFormat.IsNullOrWhiteSpace() == false && actualValue is DateTime)
                {
                    Assert.AreEqual(((DateTime)expectedValue).ToString(dateTimeFormat), ((DateTime)actualValue).ToString(dateTimeFormat), "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                }
                else
                {
                    Assert.AreEqual(expectedValue, actualValue, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                }
            }
        }


    }
}