using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class TypeLoaderTests
    {
        private TypeLoader _typeLoader;

        [SetUp]
        public void Initialize()
        {
            // this ensures it's reset
            _typeLoader = new TypeLoader(NoAppCache.Instance, IOHelper.MapPath("~/App_Data/TEMP"), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()), false);

            // for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            // TODO: Should probably update this so it only searches this assembly and add custom types to be found
            _typeLoader.AssembliesToScan = new[]
            {
                this.GetType().Assembly,
                typeof(System.Guid).Assembly,
                typeof(NUnit.Framework.Assert).Assembly,
                typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
                typeof(System.Xml.NameTable).Assembly,
                typeof(System.Configuration.GenericEnumConverter).Assembly,
                typeof(System.Web.SiteMap).Assembly,
                //typeof(TabPage).Assembly,
                typeof(System.Web.Mvc.ActionResult).Assembly,
                typeof(TypeFinder).Assembly,
                typeof(UmbracoContext).Assembly
            };
        }

        [TearDown]
        public void TearDown()
        {
            _typeLoader = null;


            // cleanup
            var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var tlDir = Path.Combine(assDir.FullName, "TypeLoader");
            if (!Directory.Exists(tlDir))
                return;
            Directory.Delete(tlDir, true);
        }

        private DirectoryInfo PrepareFolder()
        {
            var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var tlDir = Path.Combine(assDir.FullName, "TypeLoader");
            var dir = Directory.CreateDirectory(Path.Combine(tlDir, Guid.NewGuid().ToString("N")));
            return dir;
        }

        //[Test]
        //public void Scan_Vs_Load_Benchmark()
        //{
        //    var typeLoader = new TypeLoader(false);
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //    watch.Reset();
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var refreshers = typeLoader.GetTypes<ICacheRefresher>(false);
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        //}

        ////NOTE: This test shows that Type.GetType is 100% faster than Assembly.Load(..).GetType(...) so we'll use that :)
        //[Test]
        //public void Load_Type_Benchmark()
        //{
        //    var watch = new Stopwatch();
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //        var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //    watch.Reset();
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var type2 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //            .GetType("umbraco.macroCacheRefresh");
        //        var type3 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //            .GetType("umbraco.templateCacheRefresh");
        //        var type4 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //            .GetType("umbraco.presentation.cache.MediaLibraryRefreshers");
        //        var type5 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        //            .GetType("umbraco.presentation.cache.pageRefresher");
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        //    watch.Reset();
        //    watch.Start();
        //    for (var i = 0; i < 1000; i++)
        //    {
        //        var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //        var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        //    }
        //    watch.Stop();
        //    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        //}

        [Test]
        public void Detect_Legacy_Plugin_File_List()
        {
            var filePath = _typeLoader.GetTypesListFilePath();
            var fileDir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(fileDir);

            File.WriteAllText(filePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
<baseType type=""umbraco.interfaces.ICacheRefresher"">
<add type=""umbraco.macroCacheRefresh, umbraco, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null"" />
</baseType>
</plugins>");

            Assert.IsEmpty(_typeLoader.ReadCache()); // uber-legacy cannot be read

            File.Delete(filePath);

            File.WriteAllText(filePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<plugins>
<baseType type=""umbraco.interfaces.ICacheRefresher"" resolutionType=""FindAllTypes"">
<add type=""umbraco.macroCacheRefresh, umbraco, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null"" />
</baseType>
</plugins>");

            Assert.IsEmpty(_typeLoader.ReadCache()); // legacy cannot be read

            File.Delete(filePath);

            File.WriteAllText(filePath, @"IContentFinder

MyContentFinder
AnotherContentFinder

");

            Assert.IsNotNull(_typeLoader.ReadCache()); // works
        }

        [Test]
        public void Create_Cached_Plugin_File()
        {
            var types = new[] { typeof (TypeLoader), typeof (TypeLoaderTests), typeof (UmbracoContext) };

            var typeList1 = new TypeLoader.TypeList(typeof (object), null);
            foreach (var type in types) typeList1.Add(type);
            _typeLoader.AddTypeList(typeList1);
            _typeLoader.WriteCache();

            var plugins = _typeLoader.TryGetCached(typeof (object), null);
            var diffType = _typeLoader.TryGetCached(typeof (object), typeof (ObsoleteAttribute));

            Assert.IsTrue(plugins.Success);
            //this will be false since there is no cache of that type resolution kind
            Assert.IsFalse(diffType.Success);

            Assert.AreEqual(3, plugins.Result.Count());
            var shouldContain = types.Select(x => x.AssemblyQualifiedName);
            //ensure they are all found
            Assert.IsTrue(plugins.Result.ContainsAll(shouldContain));
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
            var hash1 = TypeLoader.GetFileHash(list1, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var hash2 = TypeLoader.GetFileHash(list2, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var hash3 = TypeLoader.GetFileHash(list3, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            //Assert
            Assert.AreNotEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
            Assert.AreNotEqual(hash2, hash3);

            Assert.AreEqual(hash1, TypeLoader.GetFileHash(list1, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
        }

        [Test]
        public void Ensure_Only_One_Type_List_Created()
        {
            _ = _typeLoader.ResolveFindMeTypes();
            _ = _typeLoader.ResolveFindMeTypes();
            Assert.AreEqual(1, _typeLoader.TypeLists.Count(x => x.BaseType == typeof(IFindMe) && x.AttributeType == null));
        }

        [Test]
        public void Resolves_Types()
        {
            var foundTypes1 = _typeLoader.ResolveFindMeTypes();
            Assert.AreEqual(2, foundTypes1.Count());
        }

        [Test]
        public void GetDataEditors()
        {
            var types = _typeLoader.GetDataEditors();
            Assert.AreEqual(41, types.Count());
        }

        /// <summary>
        /// This demonstrates this issue: http://issues.umbraco.org/issue/U4-3505 - the TypeList was returning a list of assignable types
        /// not explicit types which is sort of ideal but is confusing so we'll do it the less confusing way.
        /// </summary>
        [Test]
        public void TypeList_Resolves_Explicit_Types()
        {
            var types = new HashSet<TypeLoader.TypeList>();

            var propEditors = new TypeLoader.TypeList(typeof (DataEditor), null);
            propEditors.Add(typeof(LabelPropertyEditor));
            types.Add(propEditors);

            var found = types.SingleOrDefault(x => x.BaseType == typeof (DataEditor) && x.AttributeType == null);

            Assert.IsNotNull(found);

            //This should not find a type list of this type
            var shouldNotFind = types.SingleOrDefault(x => x.BaseType == typeof (IDataEditor) && x.AttributeType == null);

            Assert.IsNull(shouldNotFind);
        }

        public interface IFindMe : IDiscoverable
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
