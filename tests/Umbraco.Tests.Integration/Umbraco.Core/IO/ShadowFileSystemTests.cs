using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.IO;

[TestFixture]
[UmbracoTest]
public class ShadowFileSystemTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => ClearFiles(HostingEnvironment);

    [TearDown]
    public void TearDown() => ClearFiles(HostingEnvironment);
    // tested:
    // only 1 instance of this class is created
    // SetUp and TearDown run before/after each test
    // SetUp does not start before the previous TearDown returns

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();
    private ILogger<PhysicalFileSystem> Logger => GetRequiredService<ILogger<PhysicalFileSystem>>();

    private void ClearFiles(IHostingEnvironment hostingEnvironment)
    {
        TestHelper.DeleteDirectory(hostingEnvironment.MapPathContentRoot("FileSysTests"));
        TestHelper.DeleteDirectory(
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
                                                  "ShadowFs"));
    }

    private static string NormPath(string path) => path.Replace('\\', Path.AltDirectorySeparatorChar);

    [Test]
    public void ShadowDeleteDirectory()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        Directory.CreateDirectory(path + "/ShadowTests/d1");
        Directory.CreateDirectory(path + "/ShadowTests/d2");

        var files = fs.GetFiles(string.Empty);
        Assert.AreEqual(0, files.Count());

        var dirs = fs.GetDirectories(string.Empty);
        Assert.AreEqual(2, dirs.Count());
        Assert.IsTrue(dirs.Contains("d1"));
        Assert.IsTrue(dirs.Contains("d2"));

        ss.DeleteDirectory("d1");

        Assert.IsTrue(Directory.Exists(path + "/ShadowTests/d1"));
        Assert.IsTrue(fs.DirectoryExists("d1"));
        Assert.IsFalse(ss.DirectoryExists("d1"));

        dirs = ss.GetDirectories(string.Empty);
        Assert.AreEqual(1, dirs.Count());
        Assert.IsTrue(dirs.Contains("d2"));
    }

    [Test]
    public void ShadowDeleteDirectoryInDir()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        Directory.CreateDirectory(path + "/ShadowTests/sub");
        Directory.CreateDirectory(path + "/ShadowTests/sub/d1");
        Directory.CreateDirectory(path + "/ShadowTests/sub/d2");

        var files = fs.GetFiles(string.Empty);
        Assert.AreEqual(0, files.Count());

        var dirs = ss.GetDirectories(string.Empty);
        Assert.AreEqual(1, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub"));

        dirs = fs.GetDirectories("sub");
        Assert.AreEqual(2, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub/d1"));
        Assert.IsTrue(dirs.Contains("sub/d2"));

        dirs = ss.GetDirectories("sub");
        Assert.AreEqual(2, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub/d1"));
        Assert.IsTrue(dirs.Contains("sub/d2"));

        ss.DeleteDirectory("sub/d1");

        Assert.IsTrue(Directory.Exists(path + "/ShadowTests/sub/d1"));
        Assert.IsTrue(fs.DirectoryExists("sub/d1"));
        Assert.IsFalse(ss.DirectoryExists("sub/d1"));

        dirs = fs.GetDirectories("sub");
        Assert.AreEqual(2, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub/d1"));
        Assert.IsTrue(dirs.Contains("sub/d2"));

        dirs = ss.GetDirectories("sub");
        Assert.AreEqual(1, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub/d2"));
    }

    [Test]
    public void ShadowDeleteFile()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

        var files = fs.GetFiles(string.Empty);
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("f1.txt"));
        Assert.IsTrue(files.Contains("f2.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("f1.txt"));
        Assert.IsTrue(files.Contains("f2.txt"));

        var dirs = ss.GetDirectories(string.Empty);
        Assert.AreEqual(0, dirs.Count());

        ss.DeleteFile("f1.txt");

        Assert.IsTrue(File.Exists(path + "/ShadowTests/f1.txt"));
        Assert.IsTrue(fs.FileExists("f1.txt"));
        Assert.IsFalse(ss.FileExists("f1.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.AreEqual(1, files.Count());
        Assert.IsTrue(files.Contains("f2.txt"));
    }

    [Test]
    public void ShadowDeleteFileInDir()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");

        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        Directory.CreateDirectory(path + "/ShadowTests/sub");
        File.WriteAllText(path + "/ShadowTests/sub/f1.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/sub/f2.txt", "foo");

        var files = fs.GetFiles("");
        Assert.AreEqual(0, files.Count());

        files = fs.GetFiles("sub");
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("sub/f1.txt"));
        Assert.IsTrue(files.Contains("sub/f2.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.AreEqual(0, files.Count());

        var dirs = ss.GetDirectories(string.Empty);
        Assert.AreEqual(1, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub"));

        files = ss.GetFiles("sub");
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("sub/f1.txt"));
        Assert.IsTrue(files.Contains("sub/f2.txt"));

        dirs = ss.GetDirectories("sub");
        Assert.AreEqual(0, dirs.Count());

        ss.DeleteFile("sub/f1.txt");

        Assert.IsTrue(File.Exists(path + "/ShadowTests/sub/f1.txt"));
        Assert.IsTrue(fs.FileExists("sub/f1.txt"));
        Assert.IsFalse(ss.FileExists("sub/f1.txt"));

        files = fs.GetFiles("sub");
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("sub/f1.txt"));
        Assert.IsTrue(files.Contains("sub/f2.txt"));

        files = ss.GetFiles("sub");
        Assert.AreEqual(1, files.Count());
        Assert.IsTrue(files.Contains("sub/f2.txt"));
    }

    [Test]
    public void ShadowCantCreateFile()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        Assert.Throws<UnauthorizedAccessException>(() =>
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
            {
                ss.AddFile("../../f1.txt", ms);
            }
        });
    }

    [Test]
    public void ShadowCreateFile()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        Assert.IsTrue(File.Exists(path + "/ShadowTests/f2.txt"));
        Assert.IsFalse(File.Exists(path + "/ShadowSystem/f2.txt"));
        Assert.IsTrue(fs.FileExists("f2.txt"));
        Assert.IsTrue(ss.FileExists("f2.txt"));

        Assert.IsFalse(File.Exists(path + "/ShadowTests/f1.txt"));
        Assert.IsTrue(File.Exists(path + "/ShadowSystem/f1.txt"));
        Assert.IsFalse(fs.FileExists("f1.txt"));
        Assert.IsTrue(ss.FileExists("f1.txt"));

        var files = ss.GetFiles(string.Empty);
        Assert.AreEqual(2, files.Count());
        Assert.IsTrue(files.Contains("f1.txt"));
        Assert.IsTrue(files.Contains("f2.txt"));

        string content;
        using (var stream = ss.OpenFile("f1.txt"))
        {
            content = new StreamReader(stream).ReadToEnd();
        }

        Assert.AreEqual("foo", content);
    }

    [Test]
    public void ShadowCreateFileInDir()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("sub/f1.txt", ms);
        }

        Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/f1.txt"));
        Assert.IsTrue(File.Exists(path + "/ShadowSystem/sub/f1.txt"));
        Assert.IsFalse(fs.FileExists("sub/f1.txt"));
        Assert.IsTrue(ss.FileExists("sub/f1.txt"));

        Assert.IsFalse(fs.DirectoryExists("sub"));
        Assert.IsTrue(ss.DirectoryExists("sub"));

        var dirs = fs.GetDirectories(string.Empty);
        Assert.AreEqual(0, dirs.Count());

        dirs = ss.GetDirectories(string.Empty);
        Assert.AreEqual(1, dirs.Count());
        Assert.IsTrue(dirs.Contains("sub"));

        var files = ss.GetFiles("sub");
        Assert.AreEqual(1, files.Count());

        string content;
        using (var stream = ss.OpenFile("sub/f1.txt"))
        {
            content = new StreamReader(stream).ReadToEnd();
        }

        Assert.AreEqual("foo", content);
    }

    [Test]
    public void ShadowAbort()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("path/to/some/dir/f1.txt", ms);
        }

        // file is only written to the shadow fs
        Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));
        Assert.IsFalse(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"));

        // let the shadow fs die
    }

    [Test]
    public void ShadowComplete()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        Directory.CreateDirectory(path + "/ShadowTests/sub/sub");
        File.WriteAllText(path + "/ShadowTests/sub/sub/f2.txt", "foo");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("path/to/some/dir/f1.txt", ms);
        }

        ss.DeleteFile("sub/sub/f2.txt");

        Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));

        ss.Complete();

        // yes we are cleaning now
        //Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt")); // *not* cleaning
        Assert.IsTrue(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"));
        Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/sub/f2.txt"));
    }

    [Test]
    public void ShadowScopeComplete()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
                                                  "ShadowFs");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(shadowfs);

        var scopedFileSystems = false;

        var phy = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path, "ignore");

        var globalSettings = Options.Create(new GlobalSettings());
        var fileSystems =
            new FileSystems(loggerFactory, IOHelper, globalSettings, HostingEnvironment)
            {
                IsScoped = () => scopedFileSystems
            };
        var shadowPath = $"x/{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        var sw = (ShadowWrapper)fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.IsTrue(phy.FileExists("sub/f1.txt"));

        string id;

        // explicit shadow without scope does not work
        sw.Shadow(id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.IsTrue(phy.FileExists("sub/f2.txt"));
        sw.UnShadow(true);
        Assert.IsTrue(phy.FileExists("sub/f2.txt"));
        Assert.IsFalse(Directory.Exists(shadowfs + "/" + id));

        // shadow with scope but no complete does not complete
        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f3.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f3.txt"));
        var dirs = Directory.GetDirectories(shadowfs);
        Assert.AreEqual(1, dirs.Length);
        Assert.AreEqual((shadowfs + "/" + id).Replace('\\', '/'), dirs[0].Replace('\\', '/'));
        dirs = Directory.GetDirectories(dirs[0]);
        var typedDir = dirs.FirstOrDefault(x => x.Replace('\\', '/').EndsWith("/x"));
        Assert.IsNotNull(typedDir);
        dirs = Directory.GetDirectories(typedDir);
        var scopedDir =
            dirs.FirstOrDefault(x => x.Replace('\\', '/').EndsWith("/" + shadowPath)); // this is where files go
        Assert.IsNotNull(scopedDir);
        scope.Dispose();
        scopedFileSystems = false;
        Assert.IsFalse(phy.FileExists("sub/f3.txt"));
        TestHelper.TryAssert(() => Assert.IsFalse(Directory.Exists(shadowfs + "/" + id)));

        // shadow with scope and complete does complete
        scopedFileSystems = true; // pretend we have a scope
        scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f4.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f4.txt"));
        Assert.AreEqual(1, Directory.GetDirectories(shadowfs).Length);
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        TestHelper.TryAssert(() => Assert.AreEqual(0, Directory.GetDirectories(shadowfs).Length));
        Assert.IsTrue(phy.FileExists("sub/f4.txt"));
        Assert.IsFalse(Directory.Exists(shadowfs + "/" + id));

        // test scope for "another thread"

        scopedFileSystems = true; // pretend we have a scope
        scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f5.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f5.txt"));

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f6.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.IsTrue(phy.FileExists("sub/f6.txt")); // other thread has written out to fs
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        Assert.IsTrue(phy.FileExists("sub/f5.txt"));
        TestHelper.TryAssert(() => Assert.IsFalse(Directory.Exists(shadowfs + "/" + id)));
    }

    [Test]
    public void ShadowScopeCompleteWithFileConflict()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
                                                  "ShadowFs");
        Directory.CreateDirectory(path);

        var scopedFileSystems = false;

        var phy = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path, "ignore");

        var globalSettings = Options.Create(new GlobalSettings());
        var fileSystems =
            new FileSystems(NullLoggerFactory.Instance, IOHelper, globalSettings, HostingEnvironment)
            {
                IsScoped = () => scopedFileSystems
            };
        var shadowPath = $"x/{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        var sw = fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.IsTrue(phy.FileExists("sub/f1.txt"));

        string id;

        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f2.txt"));

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.IsTrue(phy.FileExists("sub/f2.txt")); // other thread has written out to fs
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        Assert.IsTrue(phy.FileExists("sub/f2.txt"));
        TestHelper.TryAssert(() => Assert.IsFalse(Directory.Exists(shadowfs + "/" + id)));

        string text;
        using (var s = phy.OpenFile("sub/f2.txt"))
        using (var r = new StreamReader(s))
        {
            text = r.ReadToEnd();
        }

        // the shadow filesystem will happily overwrite anything it can
        Assert.AreEqual("foo", text);
    }

    [Test]
    public void ShadowScopeCompleteWithDirectoryConflict()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
                                                  "ShadowFs");
        Directory.CreateDirectory(path);

        var scopedFileSystems = false;

        var phy = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path, "ignore");

        var globalSettings = Options.Create(new GlobalSettings());
        var fileSystems =
            new FileSystems(NullLoggerFactory.Instance, IOHelper, globalSettings, HostingEnvironment)
            {
                IsScoped = () => scopedFileSystems
            };
        var shadowPath = $"x/{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        var sw = fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.IsTrue(phy.FileExists("sub/f1.txt"));

        string id;

        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.IsTrue(Directory.Exists(shadowfs + "/" + id));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f2.txt"));

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
        {
            sw.AddFile("sub/f2.txt/f2.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.IsTrue(phy.FileExists("sub/f2.txt/f2.txt")); // other thread has written out to fs

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f3.txt", ms);
        }

        Assert.IsFalse(phy.FileExists("sub/f3.txt"));

        scope.Complete();

        try
        {
            // no way this can work since we're trying to write a file
            // but there's now a directory with the same name on the real fs
            scope.Dispose();
            Assert.Fail("Expected AggregateException.");
        }
        catch (AggregateException ae)
        {
            Assert.AreEqual(1, ae.InnerExceptions.Count);
            var e = ae.InnerExceptions[0];
            Assert.IsNotNull(e.InnerException);
            Assert.IsInstanceOf<AggregateException>(e);
            ae = (AggregateException)e;

            Assert.AreEqual(1, ae.InnerExceptions.Count);
            e = ae.InnerExceptions[0];
            Assert.IsNotNull(e.InnerException);
            Assert.IsInstanceOf<Exception>(e.InnerException);
        }

        // still, the rest of the changes has been applied ok
        Assert.IsTrue(phy.FileExists("sub/f3.txt"));
    }

    [Test]
    public void GetFilesReturnsChildrenOnly()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        File.WriteAllText(path + "/f1.txt", "foo");
        Directory.CreateDirectory(path + "/test");
        File.WriteAllText(path + "/test/f2.txt", "foo");
        Directory.CreateDirectory(path + "/test/inner");
        File.WriteAllText(path + "/test/inner/f3.txt", "foo");

        path = NormPath(path);
        var files = Directory.GetFiles(path);
        Assert.AreEqual(1, files.Length);
        files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        Assert.AreEqual(3, files.Length);
        var efiles = Directory.EnumerateFiles(path);
        Assert.AreEqual(1, efiles.Count());
        efiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
        Assert.AreEqual(3, efiles.Count());
    }

    [Test]
    public void DeleteDirectoryAndFiles()
    {
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        File.WriteAllText(path + "/f1.txt", "foo");
        Directory.CreateDirectory(path + "/test");
        File.WriteAllText(path + "/test/f2.txt", "foo");
        Directory.CreateDirectory(path + "/test/inner");
        File.WriteAllText(path + "/test/inner/f3.txt", "foo");

        path = NormPath(path);
        TestHelper.Try(() => Directory.Delete(path, true));
        TestHelper.TryAssert(() => Assert.IsFalse(File.Exists(path + "/test/inner/f3.txt")));
    }

    /// <summary>
    ///     Check that GetFiles will return all files on the shadow, while returning
    ///     just one on each of the filesystems used by the shadow.
    /// </summary>
    [Test]
    public void ShadowGetFiles()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // Assert
        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.AreEqual(2, getFiles.Count());

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, fsFiles.Length);
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, sfsFiles.Length);
    }

    /// <summary>
    ///     Check that GetFiles using the filter function with empty string will return expected results
    /// </summary>
    [Test]
    public void ShadowGetFilesUsingEmptyFilter()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // Assert
        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.AreEqual(2, getFiles.Count());

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, fsFiles.Length);
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, sfsFiles.Length);
    }

    /// <summary>
    ///     Check that GetFiles using the filter function with null will return expected results
    /// </summary>
    [Test]
    public void ShadowGetFilesUsingNullFilter()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // Assert
        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.AreEqual(2, getFiles.Count());
        // ensure we get 2 files when using null in filter parameter
        var getFilesWithNullFilter = ss.GetFiles(string.Empty, null);
        Assert.AreEqual(2, getFilesWithNullFilter.Count());

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, fsFiles.Length);
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(1, sfsFiles.Length);
    }

    [Test]
    public void ShadowGetFilesUsingWildcardFilter()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/f2.doc", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.doc", ms);
        }

        // Assert
        // ensure we get 4 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.AreEqual(4, getFiles.Count());
        // ensure we get only 2 of 4 files from the shadow when using filter
        var getFilesWithWildcardFilter = ss.GetFiles(string.Empty, "*.doc");
        Assert.AreEqual(2, getFilesWithWildcardFilter.Count());

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(2, fsFiles.Length);
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(2, sfsFiles.Length);
    }

    [Test]
    public void ShadowGetFilesUsingSingleCharacterFilter()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/f2.doc", "foo");
        File.WriteAllText(path + "/ShadowTests/f2.docx", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.doc", ms);
        }

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.docx", ms);
        }

        // Assert
        // ensure we get 6 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.AreEqual(6, getFiles.Count());
        // ensure we get only 2 of 6 files from the shadow when using filter on shadow
        var getFilesWithWildcardSinglecharFilter = ss.GetFiles(string.Empty, "f1.d?c");
        Assert.AreEqual(1, getFilesWithWildcardSinglecharFilter.Count());
        // ensure we get only 2 of 6 files from the shadow when using filter on disk
        var getFilesWithWildcardSinglecharFilter2 = ss.GetFiles(string.Empty, "f2.d?c");
        Assert.AreEqual(1, getFilesWithWildcardSinglecharFilter2.Count());

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(3, fsFiles.Length);
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.AreEqual(3, sfsFiles.Length);
    }

    /// <summary>
    ///     Returns the full paths of the files on the disk.
    ///     Note that this will be the *actual* path of the file, meaning a file existing on the initialized FS
    ///     will be in one location, while a file written after initializing the shadow, will exist at the
    ///     shadow location directory.
    /// </summary>
    [Test]
    public void ShadowGetFullPath()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        // Assert
        var f1FullPath = ss.GetFullPath("f1.txt");
        var f2FullPath = ss.GetFullPath("f2.txt");
        Assert.AreEqual(Path.Combine(path, "ShadowTests", "f1.txt"), f1FullPath);
        Assert.AreEqual(Path.Combine(path, "ShadowSystem", "f2.txt"), f2FullPath);
    }

    /// <summary>
    ///     Returns the path relative to the filesystem root
    /// </summary>
    /// <remarks>
    ///     This file stuff in this test is kinda irrelevant with the current implementation.
    ///     We do tests that the files are written to the correct places and the relative path is returned correct,
    ///     but GetRelativePath is currently really just string manipulation so files are not actually hit by the code.
    ///     Leaving the file stuff in here for now in case the method becomes more clever at some point.
    /// </remarks>
    [Test]
    public void ShadowGetRelativePath()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "ignore");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "ignore");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        // Assert
        var f1RelativePath = ss.GetRelativePath("f1.txt");
        var f2RelativePath = ss.GetRelativePath("f2.txt");
        Assert.AreEqual("f1.txt", f1RelativePath);
        Assert.AreEqual("f2.txt", f2RelativePath);
        Assert.IsTrue(File.Exists(Path.Combine(path, "ShadowTests", "f1.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(path, "ShadowTests", "f2.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(path, "ShadowSystem", "f2.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(path, "ShadowSystem", "f1.txt")));
    }

    /// <summary>
    ///     Ensure the URL returned contains the path relative to the FS root,
    ///     but including the rootUrl the FS was initialized with.
    /// </summary>
    /// <remarks>
    ///     This file stuff in this test is kinda irrelevant with the current implementation.
    ///     We do tests that the files are written to the correct places and the URL is returned correct,
    ///     but GetUrl is currently really just string manipulation so files are not actually hit by the code.
    ///     Leaving the file stuff in here for now in case the method becomes more clever at some point.
    /// </remarks>
    [Test]
    public void ShadowGetUrl()
    {
        // Arrange
        var path = HostingEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/ShadowTests");
        Directory.CreateDirectory(path + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowTests/", "rootUrl");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, path + "/ShadowSystem/", "rootUrl");
        var ss = new ShadowFileSystem(fs, sfs);

        // Act
        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        // Assert
        var f1Url = ss.GetUrl("f1.txt");
        var f2Url = ss.GetUrl("f2.txt");
        Assert.AreEqual("rootUrl/f1.txt", f1Url);
        Assert.AreEqual("rootUrl/f2.txt", f2Url);
        Assert.IsTrue(File.Exists(Path.Combine(path, "ShadowTests", "f1.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(path, "ShadowTests", "f2.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(path, "ShadowSystem", "f2.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(path, "ShadowSystem", "f1.txt")));
    }
}
