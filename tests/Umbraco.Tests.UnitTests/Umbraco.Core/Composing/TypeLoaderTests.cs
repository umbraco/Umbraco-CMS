// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

[TestFixture]
public class TypeLoaderTests
{
    [SetUp]
    public void Initialize()
    {
        // this ensures it's reset
        var typeFinder = TestHelper.GetTypeFinder();

        // For testing, we'll specify which assemblies are scanned for the PluginTypeResolver
        // TODO: Should probably update this so it only searches this assembly and add custom types to be found
        Assembly[] assemblies =
        {
            GetType().Assembly,
            typeof(Guid).Assembly,
            typeof(Assert).Assembly,
            typeof(NameTable).Assembly,
            ////typeof(TabPage).Assembly,
            typeof(TypeFinder).Assembly, typeof(UmbracoContext).Assembly,
            typeof(CheckBoxListPropertyEditor).Assembly,
        };
        _typeLoader = new TypeLoader(
            typeFinder,
            new VaryingRuntimeHash(),
            NoAppCache.Instance,
            new DirectoryInfo(TestHelper.GetHostingEnvironment()
                .MapPathContentRoot(Constants.SystemDirectories.TempData)),
            Mock.Of<ILogger<TypeLoader>>(),
            Mock.Of<IProfiler>(),
            false,
            assemblies);
    }

    [TearDown]
    public void TearDown()
    {
        _typeLoader = null;

        // cleanup
        var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        var tlDir = Path.Combine(assDir.FullName, "TypeLoader");
        if (!Directory.Exists(tlDir))
        {
            return;
        }

        Directory.Delete(tlDir, true);
    }

    private TypeLoader _typeLoader;

    private DirectoryInfo PrepareFolder()
    {
        var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        var tlDir = Path.Combine(assDir.FullName, "TypeLoader");
        var dir = Directory.CreateDirectory(Path.Combine(tlDir, Guid.NewGuid().ToString("N")));
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

    [Test]
    public void Get_Plugins_Hash_With_Hash_Generator()
    {
        // Arrange
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
        FileInfo[] list1 = { f1, f2, f3, f4, f5, f6 };
        FileInfo[] list2 = { f1, f3, f5 };
        FileInfo[] list3 = { f1, f3, f5, f7 };

        // Act
        var hash1 = GetFileHash(list1, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));
        var hash2 = GetFileHash(list2, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));
        var hash3 = GetFileHash(list3, new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>()));

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
    ///     This demonstrates this issue: http://issues.umbraco.org/issue/U4-3505 - the TypeList was returning a list of
    ///     assignable types
    ///     not explicit types which is sort of ideal but is confusing so we'll do it the less confusing way.
    /// </summary>
    [Test]
    public void TypeList_Resolves_Explicit_Types()
    {
        var types = new HashSet<TypeLoader.TypeList>();

        var propEditors = new TypeLoader.TypeList(typeof(DataEditor), null);
        propEditors.Add(typeof(LabelPropertyEditor));
        types.Add(propEditors);

        var found = types.SingleOrDefault(x => x.BaseType == typeof(DataEditor) && x.AttributeType == null);

        Assert.IsNotNull(found);

        // This should not find a type list of this type
        var shouldNotFind = types.SingleOrDefault(x => x.BaseType == typeof(IDataEditor) && x.AttributeType == null);

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
    ///     Returns a unique hash for a combination of FileInfo objects.
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

            foreach (var fileOrFolder in filesAndFolders)
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
