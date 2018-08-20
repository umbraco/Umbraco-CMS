using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Dependencies
{
    [TestFixture]
    public class NuGet
    {
        // note
        // these tests assume that the test suite runs from the project's ~/bin directory
        // instead, we want to be able to pass the required paths as parameters
        // NUnit 3.x supports TestContext.TestParameters, alas we are still running 2.x
        //
        // so instead
        //
        // furthermore, all these tests should be parts of the build and fail the build
        // in case the NuGet things are not consisted. Also, v8 uses <PackageReference>
        // wherever possible so we will also have to deal with it eventually.

        [Test]
        public void NuGet_Package_Versions_Are_The_Same_In_All_Package_Config_Files()
        {
            var packagesAndVersions = GetNuGetPackagesInSolution();
            Assert.IsTrue(packagesAndVersions.Any());

            var failTest = false;
            foreach (var package in packagesAndVersions.OrderBy(x => x.ConfigFilePath))
            {
                var matchingPackages = packagesAndVersions.Where(x => string.Equals(x.PackageName, package.PackageName, StringComparison.InvariantCultureIgnoreCase));
                foreach (var matchingPackage in matchingPackages)
                {
                    if (package.PackageVersion == matchingPackage.PackageVersion)
                        continue;

                    Debug.WriteLine("Package '{0}' with version {1} in {2} doesn't match with version {3} in {4}",
                        package.PackageName, package.PackageVersion, package.ConfigFilePath,
                        matchingPackage.PackageVersion, matchingPackage.ConfigFilePath);
                    failTest = true;
                }
            }

            Assert.IsFalse(failTest);
        }

        [Test]
        public void NuSpec_File_Matches_Installed_Dependencies()
        {
            var nuspecsDirectory = NuSpecsDirectory;
            Assert.IsNotNull(nuspecsDirectory);

            var packagesAndVersions = GetNuGetPackagesInSolution();
            Assert.IsTrue(packagesAndVersions.Any());

            Console.WriteLine("NuSpecs: " + nuspecsDirectory.FullName);

            var failTest = false;
            var nuspecFiles = nuspecsDirectory.GetFiles().Where(x => x.Extension == ".nuspec").Select(x => x.FullName);
            foreach (var filename in nuspecFiles)
            {
                var serializer = new XmlSerializer(typeof(NuSpec));
                using (var reader = new StreamReader(filename))
                {
                    var nuspec = (NuSpec)serializer.Deserialize(reader);
                    var dependencies = nuspec.MetaData.dependencies;

                    //UmbracoCms.Core has version "[$version$]" - ignore
                    foreach (var dependency in dependencies.Where(x => x.Id != "UmbracoCms.Core"))
                    {
                        var dependencyVersionRange = dependency.Version.Replace("[", string.Empty).Replace("(", string.Empty).Split(',');
                        var dependencyMinimumVersion = dependencyVersionRange.First().Trim();

                        var matchingPackages = packagesAndVersions.Where(x => string.Equals(x.PackageName, dependency.Id,
                                        StringComparison.InvariantCultureIgnoreCase)).ToList();
                        if (matchingPackages.Any() == false)
                            continue;

                        // NuGet_Package_Versions_Are_The_Same_In_All_Package_Config_Files test
                        // guarantees that all packages have one version, solutionwide, so it's okay to take First() here
                        if (dependencyMinimumVersion != matchingPackages.First().PackageVersion)
                        {
                            Debug.WriteLine("NuSpec dependency '{0}' with minimum version {1} in doesn't match with version {2} in the solution",
                                dependency.Id, dependencyMinimumVersion, matchingPackages.First().PackageVersion);
                            failTest = true;
                        }
                    }
                }
            }

            Assert.IsFalse(failTest);
        }

        private List<PackageVersionMatcher> GetNuGetPackagesInSolution()
        {
            var packagesAndVersions = new List<PackageVersionMatcher>();
            var sourceDirectory = SourceDirectory;
            if (sourceDirectory == null) return packagesAndVersions;

            var packageConfigFiles = new List<FileInfo>();
            var packageDirectories =
                sourceDirectory.GetDirectories().Where(x =>
                            x.Name.StartsWith("Umbraco.Tests") == false).ToArray();

            foreach (var directory in packageDirectories)
            {
                var packagesFiles = directory.EnumerateFiles("packages.config");
                packageConfigFiles.AddRange(packagesFiles);
            }

            foreach (var file in packageConfigFiles)
            {
                //read all and de-duplicate packages
                var serializer = new XmlSerializer(typeof(PackageConfigEntries));
                using (var reader = new StreamReader(file.FullName))
                {
                    var packages = (PackageConfigEntries)serializer.Deserialize(reader);
                    packagesAndVersions.AddRange(packages.Package.Select(package =>
                        new PackageVersionMatcher
                        {
                            PackageName = package.Id,
                            PackageVersion = package.Version,
                            ConfigFilePath = file.FullName
                        }));
                }
            }
            return packagesAndVersions;
        }

        private DirectoryInfo SourceDirectory
        {
            get
            {
                // UMBRACO_TMP is set by VSTS and points to build.tmp
                var tmp = Environment.GetEnvironmentVariable("UMBRACO_TMP");
                if (tmp == null) return SolutionDirectory;

                var path = Path.Combine(tmp, "..\\src");
                path = Path.GetFullPath(path); // sanitize
                var dir = new DirectoryInfo(path);
                return dir.Exists ? dir : null;
            }
        }

        private DirectoryInfo NuSpecsDirectory
        {
            get
            {
                // UMBRACO_TMP is set by VSTS and points to build.tmp
                var tmp = Environment.GetEnvironmentVariable("UMBRACO_TMP");
                if (tmp == null)
                {
                    var solutionDirectory = SolutionDirectory;
                    if (solutionDirectory == null) return null;
                    return new DirectoryInfo(Path.Combine(solutionDirectory.FullName, "..\\build\\NuSpecs"));
                }

                var path = Path.Combine(tmp, "..\\build\\NuSpecs");
                path = Path.GetFullPath(path); // sanitize
                var dir = new DirectoryInfo(path);
                return dir.Exists ? dir : null;
            }
        }

        private DirectoryInfo SolutionDirectory
        {
            get
            {
                var testsDirectory = new FileInfo(TestHelper.MapPathForTest("~/"));
                if (testsDirectory.Directory == null)
                    return null;
                if (testsDirectory.Directory.Parent == null || testsDirectory.Directory.Parent.Parent == null)
                    return null;
                var solutionDirectory = testsDirectory.Directory.Parent.Parent.Parent;
                return solutionDirectory;
            }
        }
    }

    public class PackageVersionMatcher
    {
        public string ConfigFilePath { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "packages")]
    public class PackageConfigEntries
    {
        [XmlElement("package")]
        public PackagesPackage[] Package { get; set; }
    }

    [XmlType(AnonymousType = true, TypeName = "package")]
    public class PackagesPackage
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }


    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")]
    [XmlRoot(Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", IsNullable = false, ElementName = "package")]
    public class NuSpec
    {
        [XmlElement("metadata")]
        public Metadata MetaData { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", TypeName = "metadata")]
    public class Metadata
    {
        [XmlArrayItem("dependency", IsNullable = false)]
        //TODO: breaks when renamed to capital D
        public Dependency[] dependencies { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", TypeName = "dependencies")]
    public class Dependency
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }
}
