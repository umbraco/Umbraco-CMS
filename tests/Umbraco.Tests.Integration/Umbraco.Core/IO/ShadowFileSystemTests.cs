using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.IO;

[TestFixture]
[UmbracoTest]
internal sealed class ShadowFileSystemTests : UmbracoIntegrationTest
{
    [SetUp]
    public void ClearTestFilesBeforeTest() => ClearFiles();

    [TearDown]
    public void ClearTestFilesAfterTest() => ClearFiles();

    private IHostEnvironment HostEnvironment => GetRequiredService<IHostEnvironment>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private ILogger<PhysicalFileSystem> Logger => GetRequiredService<ILogger<PhysicalFileSystem>>();

    private void ClearFiles()
    {
        TestHelper.DeleteDirectory(HostEnvironment.MapPathContentRoot("FileSysTests"));
        TestHelper.DeleteDirectory(
            HostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "ShadowFs"));
    }

    /// <summary>
    /// Sets up the standard arrangement used by most tests: a <c>FileSysTests</c> root with two
    /// sibling <see cref="PhysicalFileSystem"/>s — <c>Inner</c> (the "real" file system) and
    /// <c>Staging</c> (where the shadow buffers writes) — wrapped in a <see cref="ShadowFileSystem"/>.
    /// </summary>
    private (PhysicalFileSystem Inner, PhysicalFileSystem Staging, ShadowFileSystem Shadow, string Root) CreateShadowFileSystem()
    {
        var root = HostEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(root + "/ShadowTests");
        Directory.CreateDirectory(root + "/ShadowSystem");

        var inner = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, root + "/ShadowTests/", "ignore");
        var staging = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, root + "/ShadowSystem/", "ignore");
        return (inner, staging, new ShadowFileSystem(inner, staging), root);
    }

    private static string NormPath(string path) => path.Replace('\\', Path.AltDirectorySeparatorChar);

    /// <summary>
    /// Deleting a directory through the shadow hides it from the shadow's view but does not
    /// touch the inner file system until <see cref="ShadowFileSystem.Complete"/> is called.
    /// </summary>
    [Test]
    public void Can_Delete_Directory_In_Shadow_Without_Affecting_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        Directory.CreateDirectory(path + "/ShadowTests/d1");
        Directory.CreateDirectory(path + "/ShadowTests/d2");

        var files = fs.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(0));

        var dirs = fs.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(2));
        Assert.That(dirs, Does.Contain("d1"));
        Assert.That(dirs, Does.Contain("d2"));

        ss.DeleteDirectory("d1");

        Assert.That(Directory.Exists(path + "/ShadowTests/d1"), Is.True);
        Assert.That(fs.DirectoryExists("d1"), Is.True);
        Assert.That(ss.DirectoryExists("d1"), Is.False);

        dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(1));
        Assert.That(dirs, Does.Contain("d2"));
    }

    /// <summary>
    /// Deleting a nested directory through the shadow hides only the targeted subdirectory and
    /// leaves siblings and the inner file system untouched.
    /// </summary>
    [Test]
    public void Can_Delete_Subdirectory_In_Shadow_Without_Affecting_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        Directory.CreateDirectory(path + "/ShadowTests/sub");
        Directory.CreateDirectory(path + "/ShadowTests/sub/d1");
        Directory.CreateDirectory(path + "/ShadowTests/sub/d2");

        var files = fs.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(0));

        var dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(1));
        Assert.That(dirs, Does.Contain("sub"));

        dirs = fs.GetDirectories("sub");
        Assert.That(dirs.Count(), Is.EqualTo(2));
        Assert.That(dirs, Does.Contain("sub/d1"));
        Assert.That(dirs, Does.Contain("sub/d2"));

        dirs = ss.GetDirectories("sub");
        Assert.That(dirs.Count(), Is.EqualTo(2));
        Assert.That(dirs, Does.Contain("sub/d1"));
        Assert.That(dirs, Does.Contain("sub/d2"));

        ss.DeleteDirectory("sub/d1");

        Assert.That(Directory.Exists(path + "/ShadowTests/sub/d1"), Is.True);
        Assert.That(fs.DirectoryExists("sub/d1"), Is.True);
        Assert.That(ss.DirectoryExists("sub/d1"), Is.False);

        dirs = fs.GetDirectories("sub");
        Assert.That(dirs.Count(), Is.EqualTo(2));
        Assert.That(dirs, Does.Contain("sub/d1"));
        Assert.That(dirs, Does.Contain("sub/d2"));

        dirs = ss.GetDirectories("sub");
        Assert.That(dirs.Count(), Is.EqualTo(1));
        Assert.That(dirs, Does.Contain("sub/d2"));
    }

    /// <summary>
    /// Deleting a file through the shadow hides it from the shadow's view without removing the
    /// underlying file on the inner file system.
    /// </summary>
    [Test]
    public void Can_Delete_File_In_Shadow_Without_Affecting_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

        var files = fs.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("f1.txt"));
        Assert.That(files, Does.Contain("f2.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("f1.txt"));
        Assert.That(files, Does.Contain("f2.txt"));

        var dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(0));

        ss.DeleteFile("f1.txt");

        Assert.That(File.Exists(path + "/ShadowTests/f1.txt"), Is.True);
        Assert.That(fs.FileExists("f1.txt"), Is.True);
        Assert.That(ss.FileExists("f1.txt"), Is.False);

        files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(1));
        Assert.That(files, Does.Contain("f2.txt"));
    }

    /// <summary>
    /// Moving a file through the shadow renames it in the shadow's view while leaving the
    /// inner file system unchanged.
    /// </summary>
    [Test]
    public void Can_Move_File_In_Shadow_Without_Affecting_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        var files = fs.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(1));
        Assert.That(files, Does.Contain("f1.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(1));
        Assert.That(files, Does.Contain("f1.txt"));

        var dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(0));

        ss.MoveFile("f1.txt", "f2.txt");

        Assert.That(File.Exists(path + "/ShadowTests/f1.txt"), Is.True);
        Assert.That(File.Exists(path + "/ShadowTests/f2.txt"), Is.False);
        Assert.That(fs.FileExists("f1.txt"), Is.True);
        Assert.That(fs.FileExists("f2.txt"), Is.False);
        Assert.That(ss.FileExists("f1.txt"), Is.False);
        Assert.That(ss.FileExists("f2.txt"), Is.True);

        files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(1));
        Assert.That(files, Does.Contain("f2.txt"));
    }

    /// <summary>
    /// Deleting a file inside a subdirectory through the shadow hides only the targeted file
    /// and leaves siblings and the inner file system untouched.
    /// </summary>
    [Test]
    public void Can_Delete_File_In_Subdirectory_In_Shadow_Without_Affecting_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        Directory.CreateDirectory(path + "/ShadowTests/sub");
        File.WriteAllText(path + "/ShadowTests/sub/f1.txt", "foo");
        File.WriteAllText(path + "/ShadowTests/sub/f2.txt", "foo");

        var files = fs.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(0));

        files = fs.GetFiles("sub");
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("sub/f1.txt"));
        Assert.That(files, Does.Contain("sub/f2.txt"));

        files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(0));

        var dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(1));
        Assert.That(dirs, Does.Contain("sub"));

        files = ss.GetFiles("sub");
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("sub/f1.txt"));
        Assert.That(files, Does.Contain("sub/f2.txt"));

        dirs = ss.GetDirectories("sub");
        Assert.That(dirs.Count(), Is.EqualTo(0));

        ss.DeleteFile("sub/f1.txt");

        Assert.That(File.Exists(path + "/ShadowTests/sub/f1.txt"), Is.True);
        Assert.That(fs.FileExists("sub/f1.txt"), Is.True);
        Assert.That(ss.FileExists("sub/f1.txt"), Is.False);

        files = fs.GetFiles("sub");
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("sub/f1.txt"));
        Assert.That(files, Does.Contain("sub/f2.txt"));

        files = ss.GetFiles("sub");
        Assert.That(files.Count(), Is.EqualTo(1));
        Assert.That(files, Does.Contain("sub/f2.txt"));
    }

    /// <summary>
    /// Attempting to add a file whose normalised path escapes the file system root is rejected
    /// with <see cref="UnauthorizedAccessException"/> rather than silently writing outside the
    /// sandbox.
    /// </summary>
    [Test]
    public void Cannot_Add_File_With_Path_Traversal_Outside_Root()
    {
        var (_, _, ss, _) = CreateShadowFileSystem();

        Assert.Throws<UnauthorizedAccessException>(() =>
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
            {
                ss.AddFile("../../f1.txt", ms);
            }
        });
    }

    /// <summary>
    /// New files added through the shadow are written to the staging file system only — the
    /// inner file system remains untouched until <see cref="ShadowFileSystem.Complete"/>.
    /// Existing inner files remain visible through the shadow.
    /// </summary>
    [Test]
    public void Can_Add_File_To_Shadow_Without_Touching_Inner()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        Assert.That(File.Exists(path + "/ShadowTests/f2.txt"), Is.True);
        Assert.That(File.Exists(path + "/ShadowSystem/f2.txt"), Is.False);
        Assert.That(fs.FileExists("f2.txt"), Is.True);
        Assert.That(ss.FileExists("f2.txt"), Is.True);

        Assert.That(File.Exists(path + "/ShadowTests/f1.txt"), Is.False);
        Assert.That(File.Exists(path + "/ShadowSystem/f1.txt"), Is.True);
        Assert.That(fs.FileExists("f1.txt"), Is.False);
        Assert.That(ss.FileExists("f1.txt"), Is.True);

        var files = ss.GetFiles(string.Empty);
        Assert.That(files.Count(), Is.EqualTo(2));
        Assert.That(files, Does.Contain("f1.txt"));
        Assert.That(files, Does.Contain("f2.txt"));

        string content;
        using (var stream = ss.OpenFile("f1.txt"))
        {
            content = new StreamReader(stream).ReadToEnd();
        }

        Assert.That(content, Is.EqualTo("foo"));
    }

    /// <summary>
    /// Adding a file in a new subdirectory through the shadow creates the subdirectory only in
    /// the shadow's staging area; the inner file system sees no new directory until
    /// <see cref="ShadowFileSystem.Complete"/>.
    /// </summary>
    [Test]
    public void Can_Add_File_In_New_Subdirectory_To_Shadow_Only()
    {
        var (fs, _, ss, path) = CreateShadowFileSystem();

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("sub/f1.txt", ms);
        }

        Assert.That(File.Exists(path + "/ShadowTests/sub/f1.txt"), Is.False);
        Assert.That(File.Exists(path + "/ShadowSystem/sub/f1.txt"), Is.True);
        Assert.That(fs.FileExists("sub/f1.txt"), Is.False);
        Assert.That(ss.FileExists("sub/f1.txt"), Is.True);

        Assert.That(fs.DirectoryExists("sub"), Is.False);
        Assert.That(ss.DirectoryExists("sub"), Is.True);

        var dirs = fs.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(0));

        dirs = ss.GetDirectories(string.Empty);
        Assert.That(dirs.Count(), Is.EqualTo(1));
        Assert.That(dirs, Does.Contain("sub"));

        var files = ss.GetFiles("sub");
        Assert.That(files.Count(), Is.EqualTo(1));

        string content;
        using (var stream = ss.OpenFile("sub/f1.txt"))
        {
            content = new StreamReader(stream).ReadToEnd();
        }

        Assert.That(content, Is.EqualTo("foo"));
    }

    /// <summary>
    /// Discarding a shadow without calling <see cref="ShadowFileSystem.Complete"/> leaves the
    /// inner file system untouched — staged writes only exist in the staging area.
    /// </summary>
    [Test]
    public void Can_Abandon_Shadow_Without_Affecting_Inner()
    {
        var (_, _, ss, path) = CreateShadowFileSystem();

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("path/to/some/dir/f1.txt", ms);
        }

        // file is only written to the shadow fs
        Assert.That(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"), Is.True);
        Assert.That(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"), Is.False);

        // let the shadow fs die
    }

    /// <summary>
    /// Calling <see cref="ShadowFileSystem.Complete"/> applies both staged additions and
    /// staged deletions to the inner file system.
    /// </summary>
    [Test]
    public void Can_Complete_Shadow_Applying_Add_And_Delete_To_Inner()
    {
        var (_, _, ss, path) = CreateShadowFileSystem();

        Directory.CreateDirectory(path + "/ShadowTests/sub/sub");
        File.WriteAllText(path + "/ShadowTests/sub/sub/f2.txt", "foo");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("path/to/some/dir/f1.txt", ms);
        }

        ss.DeleteFile("sub/sub/f2.txt");

        Assert.That(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"), Is.True);

        ss.Complete();

        Assert.That(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"), Is.True);
        Assert.That(File.Exists(path + "/ShadowTests/sub/sub/f2.txt"), Is.False);
    }

    /// <summary>
    /// Regression test: paths with mixed case must round-trip through
    /// <see cref="ShadowFileSystem.Complete"/> on case-sensitive file systems (Linux).
    /// Previously the shadow stored files at their original case in the staging area but
    /// tracked them under a lower-cased key, so <c>Complete()</c> looked up the staged file at
    /// the lower-cased path and failed with <see cref="FileNotFoundException"/> from
    /// <see cref="File.Move(string, string)"/>.
    /// </summary>
    [Test]
    public void Can_Complete_Shadow_With_Mixed_Case_Path()
    {
        var (_, _, ss, path) = CreateShadowFileSystem();

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("Views/PageNotFound.cshtml", ms);
        }

        // staged at original case
        Assert.That(File.Exists(path + "/ShadowSystem/Views/PageNotFound.cshtml"), Is.True);

        // case-insensitive lookups still resolve the staged file (Windows-like semantics)
        Assert.That(ss.FileExists("Views/PageNotFound.cshtml"), Is.True);
        Assert.That(ss.FileExists("views/pagenotfound.cshtml"), Is.True);

        // staging-area operations via a different-cased path must still hit the staged file
        // (canonical-path tracking; otherwise on Linux these would throw FileNotFoundException
        // or return data from a phantom second file).
        using (var stream = ss.OpenFile("views/pagenotfound.cshtml"))
        using (var reader = new StreamReader(stream))
        {
            Assert.That(reader.ReadToEnd(), Is.EqualTo("foo"));
        }

        Assert.That(ss.GetSize("VIEWS/pagenotfound.cshtml"), Is.EqualTo(3));

        // re-staging the same logical path with a different case must overwrite the
        // canonical staged file (not create a second file in the shadow on Linux)
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar-updated")))
        {
            ss.AddFile("views/pagenotfound.cshtml", ms);
        }

        // Cross-platform check: the shadow must contain exactly one file under Views.
        // (File.Exists for a different-cased path would return true on case-insensitive
        // file systems like Windows / default macOS, so we can't assert it.)
        var stagedFiles = Directory.GetFiles(path + "/ShadowSystem/Views");
        Assert.That(stagedFiles, Has.Length.EqualTo(1), "Re-staging with different case must not create a second file in the shadow.");
        Assert.That(stagedFiles[0], Does.EndWith("PageNotFound.cshtml"), "Staged file name must keep the original case.");

        ss.Complete();

        // committed at original case, with the latest content
        var committedPath = path + "/ShadowTests/Views/PageNotFound.cshtml";
        Assert.That(File.Exists(committedPath), Is.True);
        Assert.That(File.ReadAllText(committedPath), Is.EqualTo("bar-updated"));
    }

    /// <summary>
    /// Cross-platform guard for the path-case invariant. Unlike
    /// <see cref="Can_Complete_Shadow_With_Mixed_Case_Path"/>, this test does not depend on the
    /// OS being case-sensitive: it asserts the directly observable shape of the shadow's node
    /// enumeration. Previously <c>NormPath</c> lower-cased the dictionary keys, so a caller
    /// staging "Views/..." would see the shadow surface "views" — losing the original case the
    /// caller used and the case actually written to disk by the staging file system.
    /// </summary>
    [Test]
    public void Can_Preserve_Original_Path_Case_In_Staged_Shadow_Nodes()
    {
        var (_, _, ss, _) = CreateShadowFileSystem();

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("Views/PageNotFound.cshtml", ms);
        }

        var dirs = ss.GetDirectories(string.Empty).ToList();
        Assert.That(dirs, Does.Contain("Views"), "Shadow directory entry must preserve the original case used to stage the file.");
        Assert.That(dirs, Does.Not.Contain("views"), "Shadow must not surface a lower-cased duplicate of the staged directory.");
    }

    /// <summary>
    /// Verifies the full scoped-shadow lifecycle through <see cref="ShadowWrapper"/> and
    /// <see cref="ShadowFileSystems"/>: explicit shadow without a scope passes through to the
    /// real file system, a scope without <c>Complete</c> discards staged writes, a completed
    /// scope persists them, and writes from "another thread" (no ambient scope) bypass the
    /// shadow and hit the real file system directly.
    /// </summary>
    [Test]
    public void Can_Complete_Scoped_Shadow_File_Operations()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var path = HostEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
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
        var shadowPath = $"x/{Guid.NewGuid().ToString("N")[..6]}";
        var sw = (ShadowWrapper)fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f1.txt"), Is.True);

        string id;

        // explicit shadow without scope does not work
        sw.Shadow(id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f2.txt"), Is.True);
        sw.UnShadow(true);
        Assert.That(phy.FileExists("sub/f2.txt"), Is.True);
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.False);

        // shadow with scope but no complete does not complete
        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f3.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f3.txt"), Is.False);
        var dirs = Directory.GetDirectories(shadowfs);
        Assert.That(dirs, Has.Length.EqualTo(1));
        Assert.That(dirs[0].Replace('\\', '/'), Is.EqualTo((shadowfs + "/" + id).Replace('\\', '/')));
        dirs = Directory.GetDirectories(dirs[0]);
        var typedDir = dirs.FirstOrDefault(x => x.Replace('\\', '/').EndsWith("/x"));
        Assert.That(typedDir, Is.Not.Null);
        dirs = Directory.GetDirectories(typedDir);
        var scopedDir =
            dirs.FirstOrDefault(x => x.Replace('\\', '/').EndsWith("/" + shadowPath)); // this is where files go
        Assert.That(scopedDir, Is.Not.Null);
        scope.Dispose();
        scopedFileSystems = false;
        Assert.That(phy.FileExists("sub/f3.txt"), Is.False);
        TestHelper.TryAssert(() => Assert.That(Directory.Exists(shadowfs + "/" + id), Is.False));

        // shadow with scope and complete does complete
        scopedFileSystems = true; // pretend we have a scope
        scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f4.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f4.txt"), Is.False);
        Assert.That(Directory.GetDirectories(shadowfs), Has.Length.EqualTo(1));
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        TestHelper.TryAssert(() => Assert.That(Directory.GetDirectories(shadowfs), Is.Empty));
        Assert.That(phy.FileExists("sub/f4.txt"), Is.True);
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.False);

        // test scope for "another thread"

        scopedFileSystems = true; // pretend we have a scope
        scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f5.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f5.txt"), Is.False);

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f6.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.That(phy.FileExists("sub/f6.txt"), Is.True); // other thread has written out to fs
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        Assert.That(phy.FileExists("sub/f5.txt"), Is.True);
        TestHelper.TryAssert(() => Assert.That(Directory.Exists(shadowfs + "/" + id), Is.False));
    }

    /// <summary>
    /// When the real file system already contains a file written by "another thread" outside
    /// the scope, completing the shadow scope happily overwrites it — last writer wins.
    /// </summary>
    [Test]
    public void Can_Complete_Scoped_Shadow_Overwriting_Concurrent_File_Write()
    {
        var path = HostEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
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
        var shadowPath = $"x/{Guid.NewGuid().ToString("N")[..6]}";
        var sw = fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f1.txt"), Is.True);

        string id;

        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f2.txt"), Is.False);

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.That(phy.FileExists("sub/f2.txt"), Is.True); // other thread has written out to fs
        scope.Complete();
        scope.Dispose();
        scopedFileSystems = false;
        Assert.That(phy.FileExists("sub/f2.txt"), Is.True);
        TestHelper.TryAssert(() => Assert.That(Directory.Exists(shadowfs + "/" + id), Is.False));

        string text;
        using (var s = phy.OpenFile("sub/f2.txt"))
        using (var r = new StreamReader(s))
        {
            text = r.ReadToEnd();
        }

        // the shadow filesystem will happily overwrite anything it can
        Assert.That(text, Is.EqualTo("foo"));
    }

    /// <summary>
    /// When "another thread" creates a directory at a path the scope intends to write a file to,
    /// completing the scope surfaces the resulting OS error as a nested
    /// <see cref="AggregateException"/>. Non-conflicting changes within the same scope still
    /// apply successfully.
    /// </summary>
    [Test]
    [LongRunning]
    public void Cannot_Complete_Scoped_Shadow_When_Concurrent_Write_Creates_Directory_Conflict()
    {
        var path = HostEnvironment.MapPathContentRoot("FileSysTests");
        var shadowfs =
            HostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
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
        var shadowPath = $"x/{Guid.NewGuid().ToString("N")[..6]}";
        var sw = fileSystems.CreateShadowWrapper(phy, shadowPath);

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f1.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f1.txt"), Is.True);

        string id;

        scopedFileSystems = true; // pretend we have a scope
        var scope = new ShadowFileSystems(fileSystems, id = ShadowWrapper.CreateShadowId(HostingEnvironment));
        Assert.That(Directory.Exists(shadowfs + "/" + id), Is.True);
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f2.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f2.txt"), Is.False);

        // pretend we're another thread w/out scope
        scopedFileSystems = false;
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
        {
            sw.AddFile("sub/f2.txt/f2.txt", ms);
        }

        scopedFileSystems = true; // pretend we have a scope

        Assert.That(phy.FileExists("sub/f2.txt/f2.txt"), Is.True); // other thread has written out to fs

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            sw.AddFile("sub/f3.txt", ms);
        }

        Assert.That(phy.FileExists("sub/f3.txt"), Is.False);

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
            Assert.That(ae.InnerExceptions, Has.Count.EqualTo(1));
            var e = ae.InnerExceptions[0];
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e, Is.InstanceOf<AggregateException>());
            ae = (AggregateException)e;

            Assert.That(ae.InnerExceptions, Has.Count.EqualTo(1));
            e = ae.InnerExceptions[0];
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException, Is.InstanceOf<Exception>());
        }

        // still, the rest of the changes has been applied ok
        Assert.That(phy.FileExists("sub/f3.txt"), Is.True);
    }

    /// <summary>
    /// Documents the <see cref="Directory.GetFiles(string)"/> / <see cref="Directory.EnumerateFiles(string)"/>
    /// contract relied on by the shadow: the non-recursive overloads return direct children
    /// only, while the <see cref="SearchOption.AllDirectories"/> overload walks the tree.
    /// </summary>
    [Test]
    public void Can_Enumerate_Direct_Children_Only_With_Default_Search_Option()
    {
        var path = HostEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        File.WriteAllText(path + "/f1.txt", "foo");
        Directory.CreateDirectory(path + "/test");
        File.WriteAllText(path + "/test/f2.txt", "foo");
        Directory.CreateDirectory(path + "/test/inner");
        File.WriteAllText(path + "/test/inner/f3.txt", "foo");

        path = NormPath(path);
        var files = Directory.GetFiles(path);
        Assert.That(files, Has.Length.EqualTo(1));
        files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        Assert.That(files, Has.Length.EqualTo(3));
        var efiles = Directory.EnumerateFiles(path);
        Assert.That(efiles.Count(), Is.EqualTo(1));
        efiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
        Assert.That(efiles.Count(), Is.EqualTo(3));
    }

    /// <summary>
    /// Sanity check that recursive <see cref="Directory.Delete(string, bool)"/> removes
    /// nested files as well as the directories — relied on by shadow cleanup.
    /// </summary>
    [Test]
    public void Can_Delete_Directory_Recursively_Including_Nested_Files()
    {
        var path = HostEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(path);
        File.WriteAllText(path + "/f1.txt", "foo");
        Directory.CreateDirectory(path + "/test");
        File.WriteAllText(path + "/test/f2.txt", "foo");
        Directory.CreateDirectory(path + "/test/inner");
        File.WriteAllText(path + "/test/inner/f3.txt", "foo");

        path = NormPath(path);
        TestHelper.Try(() => Directory.Delete(path, true));
        TestHelper.TryAssert(() => Assert.That(File.Exists(path + "/test/inner/f3.txt"), Is.False));
    }

    /// <summary>
    /// <see cref="ShadowFileSystem.GetFiles(string)"/> returns the union of inner files and
    /// staged additions, while each underlying file system still sees only the files written
    /// against it directly.
    /// </summary>
    [Test]
    public void Can_Get_Files_From_Shadow_Returning_Union_Of_Inner_And_Staging()
    {
        var (fs, sfs, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.That(getFiles.Count(), Is.EqualTo(2));

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.That(fsFiles, Has.Length.EqualTo(1));
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.That(sfsFiles, Has.Length.EqualTo(1));
    }

    /// <summary>
    /// Calling <see cref="ShadowFileSystem.GetFiles(string,string?)"/> with an empty filter
    /// returns the same union as the no-filter overload.
    /// </summary>
    [Test]
    public void Can_Get_Files_From_Shadow_With_Empty_Filter()
    {
        var (fs, sfs, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.That(getFiles.Count(), Is.EqualTo(2));

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.That(fsFiles, Has.Length.EqualTo(1));
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.That(sfsFiles, Has.Length.EqualTo(1));
    }

    /// <summary>
    /// Calling <see cref="ShadowFileSystem.GetFiles(string,string?)"/> with a <c>null</c>
    /// filter behaves the same as omitting the filter.
    /// </summary>
    [Test]
    public void Can_Get_Files_From_Shadow_With_Null_Filter()
    {
        var (fs, sfs, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f1.txt", ms);
        }

        // ensure we get 2 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.That(getFiles.Count(), Is.EqualTo(2));

        // ensure we get 2 files when using null in filter parameter
        var getFilesWithNullFilter = ss.GetFiles(string.Empty, null);
        Assert.That(getFilesWithNullFilter.Count(), Is.EqualTo(2));

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.That(fsFiles, Has.Length.EqualTo(1));
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.That(sfsFiles, Has.Length.EqualTo(1));
    }

    /// <summary>
    /// A <c>*</c> wildcard filter passed to <see cref="ShadowFileSystem.GetFiles(string,string?)"/>
    /// restricts both inner and staged files to the matching extension.
    /// </summary>
    [Test]
    public void Can_Get_Files_From_Shadow_With_Wildcard_Filter()
    {
        var (fs, sfs, ss, path) = CreateShadowFileSystem();

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

        // ensure we get 4 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.That(getFiles.Count(), Is.EqualTo(4));

        // ensure we get only 2 of 4 files from the shadow when using filter
        var getFilesWithWildcardFilter = ss.GetFiles(string.Empty, "*.doc");
        Assert.That(getFilesWithWildcardFilter.Count(), Is.EqualTo(2));

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.That(fsFiles, Has.Length.EqualTo(2));
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.That(sfsFiles, Has.Length.EqualTo(2));
    }

    /// <summary>
    /// A <c>?</c> single-character wildcard in the filter passed to
    /// <see cref="ShadowFileSystem.GetFiles(string,string?)"/> matches exactly one character —
    /// e.g. <c>f1.d?c</c> matches <c>f1.doc</c> but not <c>f1.docx</c>.
    /// </summary>
    [Test]
    public void Can_Get_Files_From_Shadow_With_Single_Character_Wildcard_Filter()
    {
        var (fs, sfs, ss, path) = CreateShadowFileSystem();

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

        // ensure we get 6 files from the shadow
        var getFiles = ss.GetFiles(string.Empty);
        Assert.That(getFiles.Count(), Is.EqualTo(6));

        // ensure we get only 1 of 6 files from the shadow when using filter on shadow
        var getFilesWithWildcardSinglecharFilter = ss.GetFiles(string.Empty, "f1.d?c");
        Assert.That(getFilesWithWildcardSinglecharFilter.Count(), Is.EqualTo(1));

        // ensure we get only 1 of 6 files from the shadow when using filter on disk
        var getFilesWithWildcardSinglecharFilter2 = ss.GetFiles(string.Empty, "f2.d?c");
        Assert.That(getFilesWithWildcardSinglecharFilter2.Count(), Is.EqualTo(1));

        var fsFiles = fs.GetFiles(string.Empty).ToArray();
        Assert.That(fsFiles, Has.Length.EqualTo(3));
        var sfsFiles = sfs.GetFiles(string.Empty).ToArray();
        Assert.That(sfsFiles, Has.Length.EqualTo(3));
    }

    /// <summary>
    /// <see cref="ShadowFileSystem.GetFullPath(string)"/> returns the path of the file at its
    /// actual location: inner-resident files resolve to the inner file system, staged files
    /// resolve to the staging file system.
    /// </summary>
    [Test]
    public void Can_Get_Full_Path_Returning_Inner_Or_Staging_Location()
    {
        var (_, _, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        var f1FullPath = ss.GetFullPath("f1.txt");
        var f2FullPath = ss.GetFullPath("f2.txt");
        Assert.That(f1FullPath, Is.EqualTo(Path.Combine(path, "ShadowTests", "f1.txt")));
        Assert.That(f2FullPath, Is.EqualTo(Path.Combine(path, "ShadowSystem", "f2.txt")));
    }

    /// <summary>
    /// <see cref="ShadowFileSystem.GetRelativePath(string)"/> returns the path relative to the
    /// shadow's root regardless of whether the file is inner-resident or staged. The current
    /// implementation is string manipulation only — the on-disk files are present here just to
    /// document the expected layout in case the method evolves.
    /// </summary>
    [Test]
    public void Can_Get_Relative_Path_From_Shadow()
    {
        var (_, _, ss, path) = CreateShadowFileSystem();

        File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        var f1RelativePath = ss.GetRelativePath("f1.txt");
        var f2RelativePath = ss.GetRelativePath("f2.txt");
        Assert.That(f1RelativePath, Is.EqualTo("f1.txt"));
        Assert.That(f2RelativePath, Is.EqualTo("f2.txt"));
        Assert.That(File.Exists(Path.Combine(path, "ShadowTests", "f1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(path, "ShadowTests", "f2.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(path, "ShadowSystem", "f2.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(path, "ShadowSystem", "f1.txt")), Is.False);
    }

    /// <summary>
    /// <see cref="ShadowFileSystem.GetUrl(string?)"/> returns the URL relative to the FS root,
    /// prefixed with the <c>rootUrl</c> the inner file system was constructed with. As with
    /// <see cref="Can_Get_Relative_Path_From_Shadow"/>, the on-disk files are illustrative.
    /// </summary>
    [Test]
    public void Can_Get_Url_From_Shadow()
    {
        var root = HostEnvironment.MapPathContentRoot("FileSysTests");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(root + "/ShadowTests");
        Directory.CreateDirectory(root + "/ShadowSystem");

        var fs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, root + "/ShadowTests/", "rootUrl");
        var sfs = new PhysicalFileSystem(IOHelper, HostingEnvironment, Logger, root + "/ShadowSystem/", "rootUrl");
        var ss = new ShadowFileSystem(fs, sfs);

        File.WriteAllText(root + "/ShadowTests/f1.txt", "foo");
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            ss.AddFile("f2.txt", ms);
        }

        var f1Url = ss.GetUrl("f1.txt");
        var f2Url = ss.GetUrl("f2.txt");
        Assert.That(f1Url, Is.EqualTo("rootUrl/f1.txt"));
        Assert.That(f2Url, Is.EqualTo("rootUrl/f2.txt"));
        Assert.That(File.Exists(Path.Combine(root, "ShadowTests", "f1.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(root, "ShadowTests", "f2.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(root, "ShadowSystem", "f2.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(root, "ShadowSystem", "f1.txt")), Is.False);
    }
}
