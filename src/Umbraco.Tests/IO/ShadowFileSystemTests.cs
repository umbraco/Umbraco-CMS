using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core.IO;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class ShadowFileSystemTests
    {
        [SetUp]
        public void SetUp()
        { }

        [TearDown]
        public void TearDown()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            if (Directory.Exists(path) == false) return;
            Directory.Delete(path, true);
        }

        private static string NormPath(string path)
        {
            return path.ToLowerInvariant().Replace("\\", "/");
        }

        [Test]
        public void ShadowDeleteDirectory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/d1");
            Directory.CreateDirectory(path + "/ShadowTests/d2");

            var files = fs.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = fs.GetDirectories("");
            Assert.AreEqual(2, dirs.Count());
            Assert.IsTrue(dirs.Contains("d1"));
            Assert.IsTrue(dirs.Contains("d2"));

            ss.DeleteDirectory("d1");

            Assert.IsTrue(Directory.Exists(path + "/ShadowTests/d1"));
            Assert.IsTrue(fs.DirectoryExists("d1"));
            Assert.IsFalse(ss.DirectoryExists("d1"));

            dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("d2"));
        }

        [Test]
        public void ShadowDeleteDirectoryInDir()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/sub");
            Directory.CreateDirectory(path + "/ShadowTests/sub/d1");
            Directory.CreateDirectory(path + "/ShadowTests/sub/d2");

            var files = fs.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = ss.GetDirectories("");
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
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
            File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

            var files = fs.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            files = ss.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            var dirs = ss.GetDirectories("");
            Assert.AreEqual(0, dirs.Count());

            ss.DeleteFile("f1.txt");

            Assert.IsTrue(File.Exists(path + "/ShadowTests/f1.txt"));
            Assert.IsTrue(fs.FileExists("f1.txt"));
            Assert.IsFalse(ss.FileExists("f1.txt"));

            files = ss.GetFiles("");
            Assert.AreEqual(1, files.Count());
            Assert.IsTrue(files.Contains("f2.txt"));
        }

        [Test]
        public void ShadowDeleteFileInDir()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
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

            files = ss.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = ss.GetDirectories("");
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
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Assert.Throws<FileSecurityException>(() =>
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                    ss.AddFile("../../f1.txt", ms);
            });
        }

        [Test]
        public void ShadowCreateFile()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("f1.txt", ms);

            Assert.IsTrue(File.Exists(path + "/ShadowTests/f2.txt"));
            Assert.IsFalse(File.Exists(path + "/ShadowSystem/f2.txt"));
            Assert.IsTrue(fs.FileExists("f2.txt"));
            Assert.IsTrue(ss.FileExists("f2.txt"));

            Assert.IsFalse(File.Exists(path + "/ShadowTests/f1.txt"));
            Assert.IsTrue(File.Exists(path + "/ShadowSystem/f1.txt"));
            Assert.IsFalse(fs.FileExists("f1.txt"));
            Assert.IsTrue(ss.FileExists("f1.txt"));

            var files = ss.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            string content;
            using (var stream = ss.OpenFile("f1.txt"))
                content = new StreamReader(stream).ReadToEnd();

            Assert.AreEqual("foo", content);
        }

        [Test]
        public void ShadowCreateFileInDir()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("sub/f1.txt", ms);

            Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/f1.txt"));
            Assert.IsTrue(File.Exists(path + "/ShadowSystem/sub/f1.txt"));
            Assert.IsFalse(fs.FileExists("sub/f1.txt"));
            Assert.IsTrue(ss.FileExists("sub/f1.txt"));

            Assert.IsFalse(fs.DirectoryExists("sub"));
            Assert.IsTrue(ss.DirectoryExists("sub"));

            var dirs = fs.GetDirectories("");
            Assert.AreEqual(0, dirs.Count());

            dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub"));

            var files = ss.GetFiles("sub");
            Assert.AreEqual(1, files.Count());

            string content;
            using (var stream = ss.OpenFile("sub/f1.txt"))
                content = new StreamReader(stream).ReadToEnd();

            Assert.AreEqual("foo", content);
        }

        [Test]
        public void ShadowAbort()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("path/to/some/dir/f1.txt", ms);

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));

            // kill everything and let the shadow fs die
            Directory.Delete(path + "/ShadowSystem", true);
        }

        [Test]
        public void ShadowComplete()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/sub/sub");
            File.WriteAllText(path + "/ShadowTests/sub/sub/f2.txt", "foo");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("path/to/some/dir/f1.txt", ms);
            ss.DeleteFile("sub/sub/f2.txt");

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));

            ss.Complete();

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt")); // *not* cleaning
            Assert.IsTrue(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"));
            Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/sub/f2.txt"));
        }

        [Test]
        public void GetFilesReturnsChildrenOnly()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
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
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            Directory.CreateDirectory(path);
            File.WriteAllText(path + "/f1.txt", "foo");
            Directory.CreateDirectory(path + "/test");
            File.WriteAllText(path + "/test/f2.txt", "foo");
            Directory.CreateDirectory(path + "/test/inner");
            File.WriteAllText(path + "/test/inner/f3.txt", "foo");

            path = NormPath(path);
            Directory.Delete(path, true);

            Assert.IsFalse(File.Exists(path + "/test/inner/f3.txt"));
        }
    }
}
