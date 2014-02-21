using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PropertyEditors;
using umbraco;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.editorControls;
using umbraco.interfaces;
using umbraco.uicontrols;
using umbraco.cms;

namespace Umbraco.Tests
{

    [TestFixture]
    public class PluginManagerTests
    {

        [SetUp]
        public void Initialize()
        {
            TestHelper.SetupLog4NetForTests();

            //this ensures its reset
            PluginManager.Current = new PluginManager(false);

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            //TODO: Should probably update this so it only searches this assembly and add custom types to be found
            PluginManager.Current.AssembliesToScan = new[]
			    {
			        this.GetType().Assembly, 
			        typeof(ApplicationStartupHandler).Assembly,
			        typeof(SqlCEHelper).Assembly,
			        typeof(CMSNode).Assembly,
			        typeof(System.Guid).Assembly,
			        typeof(NUnit.Framework.Assert).Assembly,
			        typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
			        typeof(System.Xml.NameTable).Assembly,
			        typeof(System.Configuration.GenericEnumConverter).Assembly,
			        typeof(System.Web.SiteMap).Assembly,
			        typeof(TabPage).Assembly,
			        typeof(System.Web.Mvc.ActionResult).Assembly,
			        typeof(TypeFinder).Assembly,
			        typeof(ISqlHelper).Assembly,
			        typeof(ICultureDictionary).Assembly,
					typeof(UmbracoContext).Assembly,
					typeof(BaseDataType).Assembly
			    };
        }

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }

        private DirectoryInfo PrepareFolder()
        {
            var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var dir = Directory.CreateDirectory(Path.Combine(assDir.FullName, "PluginManager", Guid.NewGuid().ToString("N")));
            foreach (var f in dir.GetFiles())
            {
                f.Delete();
            }
            return dir;
        }

        //[Test]
        //public void Scan_Vs_Load_Benchmark()
        //{
        //	var pluginManager = new PluginManager(false);
        //	var watch = new Stopwatch();
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //	watch.Reset();
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var refreshers = pluginManager.ResolveTypes<ICacheRefresher>(false);
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        //}

        ////NOTE: This test shows that Type.GetType is 100% faster than Assembly.Load(..).GetType(...) so we'll use that :)
        //[Test]
        //public void Load_Type_Benchmark()
        //{
        //	var watch = new Stopwatch();
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //		var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //	watch.Reset();
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var type2 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //			.GetType("umbraco.macroCacheRefresh");
        //		var type3 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //			.GetType("umbraco.templateCacheRefresh");
        //		var type4 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //			.GetType("umbraco.presentation.cache.MediaLibraryRefreshers");
        //		var type5 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //			.GetType("umbraco.presentation.cache.pageRefresher");
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        //	watch.Reset();
        //	watch.Start();
        //	for (var i = 0; i < 1000; i++)
        //	{
        //		var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //		var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //	}
        //	watch.Stop();
        //	Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //}

        [Test]
        public void Detect_Legacy_Plugin_File_List()
        {
            var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/PluginCache");
            var manager = new PluginManager(false);
            var filePath= Path.Combine(tempFolder, string.Format("umbraco-plugins.{0}.list", NetworkHelper.FileSafeMachineName));

            File.WriteAllText(filePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
<baseType type=""umbraco.interfaces.ICacheRefresher"">
<add type=""umbraco.macroCacheRefresh, umbraco, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null"" />
</baseType>
</plugins>");
            
            Assert.IsTrue(manager.DetectLegacyPluginListFile());

            File.Delete(filePath);

            //now create a valid one
            File.WriteAllText(filePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
<baseType type=""umbraco.interfaces.ICacheRefresher"" resolutionType=""FindAllTypes"">
<add type=""umbraco.macroCacheRefresh, umbraco, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null"" />
</baseType>
</plugins>");

            Assert.IsFalse(manager.DetectLegacyPluginListFile());
        }

        [Test]
        public void Create_Cached_Plugin_File()
        {
            var types = new[] { typeof(PluginManager), typeof(PluginManagerTests), typeof(UmbracoContext) };

            var manager = new PluginManager(false);
            //yes this is silly, none of these types inherit from string, but this is just to test the xml file format
            manager.UpdateCachedPluginsFile<string>(types, PluginManager.TypeResolutionKind.FindAllTypes);

            var plugins = manager.TryGetCachedPluginsFromFile<string>(PluginManager.TypeResolutionKind.FindAllTypes);
            var diffType = manager.TryGetCachedPluginsFromFile<string>(PluginManager.TypeResolutionKind.FindAttributedTypes);

            Assert.IsTrue(plugins.Success);
            //this will be false since there is no cache of that type resolution kind
            Assert.IsFalse(diffType.Success);

            Assert.AreEqual(3, plugins.Result.Count());
            var shouldContain = types.Select(x => x.AssemblyQualifiedName);
            //ensure they are all found
            Assert.IsTrue(plugins.Result.ContainsAll(shouldContain));
        }

        [Test]
        public void PluginHash_From_String()
        {
            var s = "hello my name is someone".GetHashCode().ToString("x", CultureInfo.InvariantCulture);
            var output = PluginManager.ConvertPluginsHashFromHex(s);
            Assert.AreNotEqual(0, output);
        }

        [Test]
        public void Get_Plugins_Hash()
        {
            //Arrange
            var dir = PrepareFolder();
            var d1 = dir.CreateSubdirectory("1");
            var d2 = dir.CreateSubdirectory("2");
            var d3 = dir.CreateSubdirectory("3");
            var d4 = dir.CreateSubdirectory("4");
            var f1 = new FileInfo(Path.Combine(d1.FullName, "test1.dll"));
            var f2 = new FileInfo(Path.Combine(d1.FullName, "test2.dll"));
            var f3 = new FileInfo(Path.Combine(d2.FullName, "test1.dll"));
            var f4 = new FileInfo(Path.Combine(d2.FullName, "test2.dll"));
            var f5 = new FileInfo(Path.Combine(d3.FullName, "test1.dll"));
            var f6 = new FileInfo(Path.Combine(d3.FullName, "test2.dll"));
            var f7 = new FileInfo(Path.Combine(d4.FullName, "test1.dll"));
            f1.CreateText().Close();
            f2.CreateText().Close();
            f3.CreateText().Close();
            f4.CreateText().Close();
            f5.CreateText().Close();
            f6.CreateText().Close();
            f7.CreateText().Close();
            var list1 = new[] { f1, f2, f3, f4, f5, f6 };
            var list2 = new[] { f1, f3, f5 };
            var list3 = new[] { f1, f3, f5, f7 };

            //Act
            var hash1 = PluginManager.GetFileHash(list1);
            var hash2 = PluginManager.GetFileHash(list2);
            var hash3 = PluginManager.GetFileHash(list3);

            //Assert
            Assert.AreNotEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
            Assert.AreNotEqual(hash2, hash3);

            Assert.AreEqual(hash1, PluginManager.GetFileHash(list1));
        }

        [Test]
        public void Ensure_Only_One_Type_List_Created()
        {
            var foundTypes1 = PluginManager.Current.ResolveFindMeTypes();
            var foundTypes2 = PluginManager.Current.ResolveFindMeTypes();
            Assert.AreEqual(1,
                            PluginManager.Current.GetTypeLists()
                                .Count(x => x.IsTypeList<IFindMe>(PluginManager.TypeResolutionKind.FindAllTypes)));
        }

        [Test]
        public void Resolves_Assigned_Mappers()
        {
            var foundTypes1 = PluginManager.Current.ResolveAssignedMapperTypes();
            Assert.AreEqual(21, foundTypes1.Count());
        }

        [Test]
        public void Resolves_Types()
        {
            var foundTypes1 = PluginManager.Current.ResolveFindMeTypes();
            Assert.AreEqual(2, foundTypes1.Count());
        }

        [Test]
        public void Resolves_Attributed_Trees()
        {
            var trees = PluginManager.Current.ResolveAttributedTrees();
            Assert.AreEqual(19, trees.Count());
        }

        [Test]
        public void Resolves_Actions()
        {
            var actions = PluginManager.Current.ResolveActions();
            Assert.AreEqual(36, actions.Count());
        }

        [Test]
        public void Resolves_Trees()
        {
            var trees = PluginManager.Current.ResolveTrees();
            Assert.AreEqual(39, trees.Count());
        }

        [Test]
        public void Resolves_Applications()
        {
            var apps = PluginManager.Current.ResolveApplications();
            Assert.AreEqual(7, apps.Count());
        }

        [Test]
        public void Resolves_DataTypes()
        {
            var types = PluginManager.Current.ResolveDataTypes();
            Assert.AreEqual(35, types.Count());
        }

        [Test]
        public void Resolves_RazorDataTypeModels()
        {
            var types = PluginManager.Current.ResolveRazorDataTypeModels();
            Assert.AreEqual(2, types.Count());
        }

        [Test]
        public void Resolves_RestExtensions()
        {
            var types = PluginManager.Current.ResolveRestExtensions();
            Assert.AreEqual(3, types.Count());
        }

        [Test]
        public void Resolves_XsltExtensions()
        {
            var types = PluginManager.Current.ResolveXsltExtensions();
            Assert.AreEqual(3, types.Count());
        }

        /// <summary>
        /// This demonstrates this issue: http://issues.umbraco.org/issue/U4-3505 - the TypeList was returning a list of assignable types
        /// not explicit types which is sort of ideal but is confusing so we'll do it the less confusing way.
        /// </summary>
        [Test]
        public void TypeList_Resolves_Explicit_Types()
        {
            var types = new HashSet<PluginManager.TypeList>();

            var propEditors = new PluginManager.TypeList<PropertyEditor>(PluginManager.TypeResolutionKind.FindAllTypes);
            propEditors.AddType(typeof(LabelPropertyEditor));
            types.Add(propEditors);

            var found = types.SingleOrDefault(x => x.IsTypeList<PropertyEditor>(PluginManager.TypeResolutionKind.FindAllTypes));

            Assert.IsNotNull(found);

            //This should not find a type list of this type
            var shouldNotFind = types.SingleOrDefault(x => x.IsTypeList<IParameterEditor>(PluginManager.TypeResolutionKind.FindAllTypes));

            Assert.IsNull(shouldNotFind);
        }
     
        [XsltExtension("Blah.Blah")]
        public class MyXsltExtension
        {

        }


        [Umbraco.Web.BaseRest.RestExtension("Blah")]
        public class MyRestExtesion
        {

        }

        public interface IFindMe
        {

        }

        public class FindMe1 : IFindMe
        {

        }

        public class FindMe2 : IFindMe
        {

        }

    }
}