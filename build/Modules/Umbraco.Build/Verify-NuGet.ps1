#
# Verify-NuGet
#

function Format-Dependency
{
  param ( $d )
  
  $m = $d.Id + " "
  if ($d.MinInclude) { $m = $m + "[" }
  else { $m = $m + "(" }
  $m = $m + $d.MinVersion
  if ($d.MaxVersion -ne $d.MinVersion) { $m = $m + "," + $d.MaxVersion }
  if ($d.MaxInclude) { $m = $m + "]" }
  else { $m = $m + ")" }

  return $m
}

function Write-NuSpec
{
  param ( $name, $deps )

  Write-Host ""
  Write-Host "$name NuSpec dependencies:"
  
  foreach ($d in $deps)
  {
    $m = Format-Dependency $d
    Write-Host " $m"
  }
}

function Write-Package
{
  param ( $name, $pkgs )
  
  Write-Host ""
  Write-Host "$name packages:"
  
  foreach ($p in $pkgs)
  {
    Write-Host " $($p.Id) $($p.Version)"
  }
}

function Verify-NuGet
{
  param (
    $uenv # an Umbraco build environment (see Get-UmbracoBuildEnv)
  )
  
  if ($uenv -eq $null)
  {
    $uenv = Get-UmbracoBuildEnv
  }

  $source = @"
  
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Semver;
  
    namespace Umbraco.Build
    {
      public class NuGet
      {       
        public static Dependency[] GetNuSpecDependencies(string filename)
        {
          NuSpec nuspec;
          var serializer = new XmlSerializer(typeof(NuSpec));
          using (var reader = new StreamReader(filename))
          {
            nuspec = (NuSpec) serializer.Deserialize(reader);
          }
          var nudeps = nuspec.Metadata.Dependencies;
          var deps = new List<Dependency>();
          foreach (var nudep in nudeps)
          {
            var dep = new Dependency();
            dep.Id = nudep.Id;
            
            var parts = nudep.Version.Split(',');
            if (parts.Length == 1)
            {
              dep.MinInclude = parts[0].StartsWith("[");
              dep.MaxInclude = parts[0].EndsWith("]");
              
              SemVersion version;
              if (!SemVersion.TryParse(parts[0].Substring(1, parts[0].Length-2).Trim(), out version)) continue;
              dep.MinVersion = dep.MaxVersion = version; //parts[0].Substring(1, parts[0].Length-2).Trim();
            }
            else
            {
              SemVersion version;
              if (!SemVersion.TryParse(parts[0].Substring(1).Trim(), out version)) continue;
              dep.MinVersion = version; //parts[0].Substring(1).Trim();
              if (!SemVersion.TryParse(parts[1].Substring(0, parts[1].Length-1).Trim(), out version)) continue;
              dep.MaxVersion = version; //parts[1].Substring(0, parts[1].Length-1).Trim();
              dep.MinInclude = parts[0].StartsWith("[");
              dep.MaxInclude = parts[1].EndsWith("]");
            }
            
            deps.Add(dep);
          }
          return deps.ToArray();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(/*this*/ IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
          HashSet<TKey> knownKeys = new HashSet<TKey>();
          foreach (TSource element in source)
          {
            if (knownKeys.Add(keySelector(element)))
            {
              yield return element;
            }
          }
        }
        
        public static Package[] GetProjectsPackages(string src, string[] projects)
        {
          var l = new List<Package>();
          foreach (var project in projects)
          {
            var path = Path.Combine(src, project);
            var packageConfig = Path.Combine(path, "packages.config");
            if (File.Exists(packageConfig))
              ReadPackagesConfig(packageConfig, l);
            var csprojs = Directory.GetFiles(path, "*.csproj");
            foreach (var csproj in csprojs)
            {
              ReadCsProj(csproj, l);
            }
          }
          IEnumerable<Package> p = l.OrderBy(x => x.Id);
          p = DistinctBy(p, x => x.Id + ":::" + x.Version);
          return p.ToArray();
        }
        
        public static object[] GetPackageErrors(Package[] pkgs)
        {
          return pkgs
            .GroupBy(x => x.Id)
            .Where(x => x.Count() > 1)
            .ToArray();
        }
        
        public static object[] GetNuSpecErrors(Package[] pkgs, Dependency[] deps)
        {
          var d = pkgs.ToDictionary(x => x.Id, x => x.Version);
          return deps
            .Select(x => 
            {
              SemVersion v;
              if (!d.TryGetValue(x.Id, out v)) return null;
              
              var ok = true;
              
              /*
              if (x.MinInclude)
              {
                if (v < x.MinVersion) ok = false;
              }
              else
              {
                if (v <= x.MinVersion) ok = false;
              }

              if (x.MaxInclude)
              {
                if (v > x.MaxVersion) ok = false;
              }
              else
              {
                if (v >= x.MaxVersion) ok = false;
              }
              */
              
              if (!x.MinInclude || v != x.MinVersion) ok = false;
              
              return ok ? null : new { Dependency = x, Version = v };
            })
            .Where(x => x != null)
            .ToArray();
        }
        
        /*
        public static Package[] GetProjectPackages(string path)
        {
          var l = new List<Package>();
          var packageConfig = Path.Combine(path, "packages.config");
          if (File.Exists(packageConfig))
            ReadPackagesConfig(packageConfig, l);
          var csprojs = Directory.GetFiles(path, "*.csproj");
          foreach (var csproj in csprojs)
          {
            ReadCsProj(csproj, l);
          }
          return l.ToArray();
        }
        */
        
        public static string GetDirectoryName(string filename)
        {
          return Path.GetFileName(Path.GetDirectoryName(filename));
        }
        
        public static void ReadPackagesConfig(string filename, List<Package> packages)
        {
          //Console.WriteLine("read " + filename);

          PackagesConfigPackages pkgs;
          var serializer = new XmlSerializer(typeof(PackagesConfigPackages));
          using (var reader = new StreamReader(filename))
          {
            pkgs = (PackagesConfigPackages) serializer.Deserialize(reader);
          }
          foreach (var p in pkgs.Packages)
          {
            SemVersion version;
            if (!SemVersion.TryParse(p.Version, out version)) continue;
            packages.Add(new Package { Id = p.Id, Version = version, Project = GetDirectoryName(filename) });
          }
        }
        
        public static void ReadCsProj(string filename, List<Package> packages)
        {
          //Console.WriteLine("read " + filename);
          
          // if xmlns then it's not a VS2017 with PackageReference
          var text = File.ReadAllLines(filename);
          var line = text.FirstOrDefault(x => x.Contains("<Project"));
          if (line == null) return;
          if (line.Contains("xmlns")) return;         

          CsProjProject proj;
          var serializer = new XmlSerializer(typeof(CsProjProject));
          using (var reader = new StreamReader(filename))
          {
            proj = (CsProjProject) serializer.Deserialize(reader);
          }
          foreach (var p in proj.ItemGroups.Where(x => x.Packages != null).SelectMany(x => x.Packages))
          {
            var sversion = p.VersionE ?? p.VersionA;
            SemVersion version;
            if (!SemVersion.TryParse(sversion, out version)) continue;
            packages.Add(new Package { Id = p.Id, Version = version, Project = GetDirectoryName(filename) });
          }
        }
        
        public class Dependency
        {
          public string Id { get; set; }
          public SemVersion MinVersion { get; set; }
          public SemVersion MaxVersion { get; set; }
          public bool MinInclude { get; set; }
          public bool MaxInclude { get; set; }
        }
        
        public class Package
        {
          public string Id { get; set; }
          public SemVersion Version { get; set; }
          public string Project { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")]
        [XmlRoot(Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", IsNullable = false, ElementName = "package")]
        public class NuSpec
        {
          [XmlElement("metadata")]
          public NuSpecMetadata Metadata { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", TypeName = "metadata")]
        public class NuSpecMetadata
        {
          [XmlArray("dependencies")]
          [XmlArrayItem("dependency", IsNullable = false)]
          public NuSpecDependency[] Dependencies { get; set; }
        }

        [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", TypeName = "dependencies")]
        public class NuSpecDependency
        {
          [XmlAttribute(AttributeName = "id")]
          public string Id { get; set; }

          [XmlAttribute(AttributeName = "version")]
          public string Version { get; set; }
        }
        
        [XmlType(AnonymousType = true)]
        [XmlRoot(Namespace = "", IsNullable = false, ElementName = "packages")]
        public class PackagesConfigPackages
        {
            [XmlElement("package")]
            public PackagesConfigPackage[] Packages { get; set; }
        }

        [XmlType(AnonymousType = true, TypeName = "package")]
        public class PackagesConfigPackage
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public string Version { get; set; }
        }
        
        [XmlType(AnonymousType = true)]
        [XmlRoot(Namespace = "", IsNullable = false, ElementName = "Project")]
        public class CsProjProject
        {
            [XmlElement("ItemGroup")]
            public CsProjItemGroup[] ItemGroups { get; set; }
        }
        
        [XmlType(AnonymousType = true, TypeName = "ItemGroup")]
        public class CsProjItemGroup
        {
            [XmlElement("PackageReference")]
            public CsProjPackageReference[] Packages { get; set; }
        }
        
        [XmlType(AnonymousType = true, TypeName = "PackageReference")]
        public class CsProjPackageReference
        {
            [XmlAttribute(AttributeName = "Include")]
            public string Id { get; set; }

            [XmlAttribute(AttributeName = "Version")]
            public string VersionA { get; set; }
            
            [XmlElement("Version")]
            public string VersionE { get; set;}
        }
      }
    }
    
"@

  Write-Host ">> Verify NuGet consistency"

  $assem = (
    "System.Xml",
    "System.Core", # "System.Collections.Generic"
    "System.Linq",
    "System.Xml.Serialization",
    "System.IO",
    "System.Globalization",
    $uenv.Semver
  )
  
  try
  {
    # as long as the code hasn't changed it's fine to re-add, but if the code
    # has changed this will throw - better warn the dev that we have an issue
    add-type -referencedAssemblies $assem -typeDefinition $source -language CSharp
  }
  catch
  {
    if ($_.FullyQualifiedErrorId.StartsWith("TYPE_ALREADY_EXISTS,"))
      { Write-Error "Failed to add type, did you change the code?" }
    else
      { Write-Error $_ }
  }
  if (-not $?) { break }
  
  $nuspecs = (
    "UmbracoCms",
    "UmbracoCms.Core"
  )
  
  $projects = (
    "Umbraco.Core",
    "Umbraco.Web",
    "Umbraco.Web.UI",
    "UmbracoExamine"#,
    #"Umbraco.Tests",
    #"Umbraco.Tests.Benchmarks"
  )
   
  $src = "$($uenv.SolutionRoot)\src"
  $pkgs = [Umbraco.Build.NuGet]::GetProjectsPackages($src, $projects)
  if (-not $?) { break }
  #Write-Package "All" $pkgs
  
  $errs = [Umbraco.Build.NuGet]::GetPackageErrors($pkgs)
  if (-not $?) { break }
  
  if ($errs.Length -gt 0)
  {
    Write-Host ""
  }
  foreach ($err in $errs)
  {
    Write-Host $err.Key
    foreach ($e in $err)
    {
      Write-Host " $($e.Version) required by $($e.Project)"
    }
  }
  if ($errs.Length -gt 0)
  {
    Write-Error "Found non-consolidated package dependencies"
    break
  }
  
  $nuerr = $false
  $nupath = "$($uenv.SolutionRoot)\build\NuSpecs"
  foreach ($nuspec in $nuspecs)
  {  
    $deps = [Umbraco.Build.NuGet]::GetNuSpecDependencies("$nupath\$nuspec.nuspec")
    if (-not $?) { break }
    #Write-NuSpec $nuspec $deps
    
    $errs = [Umbraco.Build.NuGet]::GetNuSpecErrors($pkgs, $deps)
    if (-not $?) { break }
    
    if ($errs.Length -gt 0)
    {
      Write-Host ""
      Write-Host "$nuspec requires:"
      $nuerr = $true
    }
    foreach ($err in $errs)
    {
      $m = Format-Dependency $err.Dependency
      Write-Host " $m but projects require $($err.Version)"
    }
  }
  
  if ($nuerr)
  {
    Write-Error "Found inconsistent NuGet dependencies"
    break
  }
}