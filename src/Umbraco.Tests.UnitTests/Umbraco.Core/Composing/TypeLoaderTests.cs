// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing
{
    [TestFixture]
    public class TypeLoaderTests
    {
        private TypeLoader _typeLoader;

        [SetUp]
        public void Initialize()
        {
            // this ensures it's reset
            ITypeFinder typeFinder = TestHelper.GetTypeFinder();

            // For testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            // TODO: Should probably update this so it only searches this assembly and add custom types to be found
            Assembly[] assemblies = new[]
                {
                    GetType().Assembly,
                    typeof(Guid).Assembly,
                    typeof(Assert).Assembly,
                    typeof(System.Xml.NameTable).Assembly,
                    typeof(System.Configuration.GenericEnumConverter).Assembly,
                    ////typeof(TabPage).Assembly,
                    typeof(TypeFinder).Assembly,
                    typeof(UmbracoContext).Assembly,
                    typeof(CheckBoxListPropertyEditor).Assembly
                };
            _typeLoader = new TypeLoader(
                typeFinder,
                NoAppCache.Instance,
                new DirectoryInfo(TestHelper.GetHostingEnvironment().MapPathContentRoot(Constants.SystemDirectories.TempData)),
                Mock.Of<ILogger<TypeLoader>>(),
                new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()),
                false,
                assemblies);
        }

        [TearDown]
        public void TearDown()
        {
            _typeLoader = null;

            // cleanup
            DirectoryInfo assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string tlDir = Path.Combine(assDir.FullName, "TypeLoader");
            if (!Directory.Exists(tlDir))
            {
                return;
            }

            Directory.Delete(tlDir, true);
        }

        private DirectoryInfo PrepareFolder()
        {
            DirectoryInfo assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string tlDir = Path.Combine(assDir.FullName, "TypeLoader");
            DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(tlDir, Guid.NewGuid().ToString("N")));
            return dir;
        }

        ////[Test]
        ////public void Scan_Vs_Load_Benchmark()
        ////{
        ////    var typeLoader = new TypeLoader(false);
        ////    var watch = new Stopwatch();
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        ////    watch.Reset();
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var refreshers = typeLoader.GetTypes<ICacheRefresher>(false);
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        ////}

        //////NOTE: This test shows that Type.GetType is 100% faster than Assembly.Load(..).GetType(...) so we'll use that :)
        ////[Test]
        ////public void Load_Type_Benchmark()
        ////{
        ////    var watch = new Stopwatch();
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var type2 = Type.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type3 = Type.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type4 = Type.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////        var type5 = Type.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null");
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        ////    watch.Reset();
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var type2 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        ////            .GetType("umbraco.macroCacheRefresh");
        ////        var type3 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        ////            .GetType("umbraco.templateCacheRefresh");
        ////        var type4 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        ////            .GetType("umbraco.presentation.cache.MediaLibraryRefreshers");
        ////        var type5 = Assembly.Load("umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null")
        ////            .GetType("umbraco.presentation.cache.pageRefresher");
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (2nd round): " + watch.ElapsedMilliseconds);
        ////    watch.Reset();
        ////    watch.Start();
        ////    for (var i = 0; i < 1000; i++)
        ////    {
        ////        var type2 = BuildManager.GetType("umbraco.macroCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type3 = BuildManager.GetType("umbraco.templateCacheRefresh, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type4 = BuildManager.GetType("umbraco.presentation.cache.MediaLibraryRefreshers, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////        var type5 = BuildManager.GetType("umbraco.presentation.cache.pageRefresher, umbraco, Version=1.0.4698.259, Culture=neutral, PublicKeyToken=null", true);
        ////    }
        ////    watch.Stop();
        ////    Debug.WriteLine("TOTAL TIME (1st round): " + watch.ElapsedMilliseconds);
        ////}

        [Test]
        [Retry(5)] // TODO make this test non-flaky.
        public void Detect_Legacy_Plugin_File_List()
        {
            string filePath = _typeLoader.GetTypesListFilePath();
            string fileDir = Path.GetDirectoryName(filePath);
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

        [Retry(5)] // TODO make this test non-flaky.
        [Test]
        public void Create_Cached_Plugin_File()
        {
            Type[] types = new[] { typeof(TypeLoader), typeof(TypeLoaderTests), typeof(IUmbracoContext) };

            var typeList1 = new TypeLoader.TypeList(typeof(object), null);
            foreach (Type type in types)
            {
                typeList1.Add(type);
            }

            _typeLoader.AddTypeList(typeList1);
            _typeLoader.WriteCache();

            Attempt<IEnumerable<string>> plugins = _typeLoader.TryGetCached(typeof(object), null);
            Attempt<IEnumerable<string>> diffType = _typeLoader.TryGetCached(typeof(object), typeof(ObsoleteAttribute));

            Assert.IsTrue(plugins.Success);

            // This will be false since there is no cache of that type resolution kind
            Assert.IsFalse(diffType.Success);

            Assert.AreEqual(3, plugins.Result.Count());
            IEnumerable<string> shouldContain = types.Select(x => x.AssemblyQualifiedName);

            // Ensure they are all found
            Assert.IsTrue(plugins.Result.ContainsAll(shouldContain));
        }

        [Test]
        public void Get_Plugins_Hash_With_Hash_Generator()
        {
            // Arrange
            DirectoryInfo dir = PrepareFolder();
            DirectoryInfo d1 = dir.CreateSubdirectory("1");
            DirectoryInfo d2 = dir.CreateSubdirectory("2");
            DirectoryInfo d3 = dir.CreateSubdirectory("3");
            DirectoryInfo d4 = dir.CreateSubdirectory("4");
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
            FileInfo[] list1 = new[] { f1, f2, f3, f4, f5, f6 };
            FileInfo[] list2 = new[] { f1, f3, f5 };
            FileInfo[] list3 = new[] { f1, f3, f5, f7 };

            // Act
            string hash1 = GetFileHash(list1, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));
            string hash2 = GetFileHash(list2, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));
            string hash3 = GetFileHash(list3, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));

            // Assert
            Assert.AreNotEqual(hash1, hash2);
            Assert.AreNotEqual(hash1, hash3);
            Assert.AreNotEqual(hash2, hash3);

            Assert.AreEqual(hash1, GetFileHash(list1, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>())));
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
            IEnumerable<Type> foundTypes1 = _typeLoader.ResolveFindMeTypes();
            Assert.AreEqual(2, foundTypes1.Count());
        }

        [Test]
        public void GetDataEditors()
        {
            IEnumerable<Type> types = _typeLoader.GetDataEditors();
            Assert.AreEqual(40, types.Count());
        }

        /// <summary>
        /// This demonstrates this issue: http://issues.umbraco.org/issue/U4-3505 - the TypeList was returning a list of assignable types
        /// not explicit types which is sort of ideal but is confusing so we'll do it the less confusing way.
        /// </summary>
        [Test]
        public void TypeList_Resolves_Explicit_Types()
        {
            var types = new HashSet<TypeLoader.TypeList>();

            var propEditors = new TypeLoader.TypeList(typeof(DataEditor), null);
            propEditors.Add(typeof(LabelPropertyEditor));
            types.Add(propEditors);

            TypeLoader.TypeList found = types.SingleOrDefault(x => x.BaseType == typeof(DataEditor) && x.AttributeType == null);

            Assert.IsNotNull(found);

            // This should not find a type list of this type
            TypeLoader.TypeList shouldNotFind = types.SingleOrDefault(x => x.BaseType == typeof(IDataEditor) && x.AttributeType == null);

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

        /// <summary>
        /// Returns a unique hash for a combination of FileInfo objects.
        /// </summary>
        /// <param name="filesAndFolders">A collection of files.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <returns>The hash.</returns>
        // internal for tests
        private static string GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, IProfilingLogger logger)
        {
            using (logger.DebugDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
            {
                using var generator = new HashGenerator();

                // Get the distinct file infos to hash.
                var uniqInfos = new HashSet<string>();

                foreach (FileSystemInfo fileOrFolder in filesAndFolders)
                {
                    if (uniqInfos.Contains(fileOrFolder.FullName))
                    {
                        continue;
                    }

                    uniqInfos.Add(fileOrFolder.FullName);
                    generator.AddFileSystemItem(fileOrFolder);
                }

                return generator.GenerateHash();
            }
        }
    }
}
