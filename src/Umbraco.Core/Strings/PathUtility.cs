namespace Umbraco.Cms.Core.Strings;

public static class PathUtility
{
    /// <summary>
    ///     Ensures that a path has `~/` as prefix
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string EnsurePathIsApplicationRootPrefixed(string path)
    {
        if (path.StartsWith("~/"))
        {
            return path;
        }

        if (path.StartsWith("/") == false && path.StartsWith("\\") == false)
        {
            path = string.Format("/{0}", path);
        }

        if (path.StartsWith("~") == false)
        {
            path = string.Format("~{0}", path);
        }

        return path;
    }
}
