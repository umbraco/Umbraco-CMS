using System.Text;

namespace Umbraco.Cms.Core.Routing;

public class WebPath
{
    public const char PathSeparator = '/';

    public static string Combine(params string[]? paths)
    {
        if (paths == null)
        {
            throw new ArgumentNullException(nameof(paths));
        }

        if (paths.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        for (var index = 0; index < paths.Length; index++)
        {
            var path = paths[index];
            var start = 0;
            var count = path.Length;
            var isFirst = index == 0;
            var isLast = index == paths.Length - 1;

            // don't trim start if it's the first
            if (!isFirst && path[0] == PathSeparator)
            {
                start = 1;
            }

            // always trim end
            if (path[^1] == PathSeparator)
            {
                count = path.Length - 1;
            }

            sb.Append(path, start, count - start);

            if (!isLast)
            {
                sb.Append(PathSeparator);
            }
        }

        return sb.ToString();
    }
}
