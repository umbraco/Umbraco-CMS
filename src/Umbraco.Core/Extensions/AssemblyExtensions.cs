// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;

namespace Umbraco.Extensions;

public static class AssemblyExtensions
{
    private static string _rootDir = string.Empty;

    /// <summary>
    ///     Utility method that returns the path to the root of the application, by getting the path to where the assembly
    ///     where this
    ///     method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
    ///     even if the assembly is in a /bin/debug or /bin/release folder
    /// </summary>
    /// <returns></returns>
    public static string GetRootDirectorySafe(this Assembly executingAssembly)
    {
        if (string.IsNullOrEmpty(_rootDir) == false)
        {
            return _rootDir;
        }

        var codeBase = executingAssembly.Location;
        var uri = new Uri(codeBase);
        var path = uri.LocalPath;
        var baseDirectory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(baseDirectory))
        {
            throw new Exception(
                "No root directory could be resolved. Please ensure that your Umbraco solution is correctly configured.");
        }

        _rootDir = baseDirectory.Contains("bin")
            ? baseDirectory[..(baseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase) - 1)]
            : baseDirectory;

        return _rootDir;
    }

    /// <summary>
    ///     Returns the file used to load the assembly
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static FileInfo GetAssemblyFile(this Assembly assembly)
    {
        var codeBase = assembly.Location;
        var uri = new Uri(codeBase);
        var path = uri.LocalPath;
        return new FileInfo(path);
    }

    /// <summary>
    ///     Returns true if the assembly is the App_Code assembly
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static bool IsAppCodeAssembly(this Assembly assembly)
    {
        if (assembly.FullName!.StartsWith("App_Code"))
        {
            try
            {
                Assembly.Load("App_Code");
                return true;
            }
            catch (FileNotFoundException)
            {
                // this will occur if it cannot load the assembly
                return false;
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns true if the assembly is the compiled global asax.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static bool IsGlobalAsaxAssembly(this Assembly assembly) =>

        // only way I can figure out how to test is by the name
        assembly.FullName!.StartsWith("App_global.asax");

    /// <summary>
    ///     Returns the file used to load the assembly
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static FileInfo? GetAssemblyFile(this AssemblyName assemblyName)
    {
        var codeBase = assemblyName.CodeBase;
        if (!string.IsNullOrEmpty(codeBase))
        {
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            return new FileInfo(path);
        }

        return null;
    }
}
