// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Extensions;

public static class VersionExtensions
{
    public static Version GetVersion(this SemVersion semVersion, int maxParts = 4)
    {
        int.TryParse(semVersion.Build, NumberStyles.Integer, CultureInfo.InvariantCulture, out int build);

        if (maxParts >= 4)
        {
            return new Version(semVersion.Major, semVersion.Minor, semVersion.Patch, build);
        }

        if (maxParts == 3)
        {
            return new Version(semVersion.Major, semVersion.Minor, semVersion.Patch);
        }

        return new Version(semVersion.Major, semVersion.Minor);
    }

    public static Version SubtractRevision(this Version version)
    {
        var parts = new List<int>(new[] { version.Major, version.Minor, version.Build, version.Revision });

        // remove all prefixed zero parts
        while (parts[0] <= 0)
        {
            parts.RemoveAt(0);
            if (parts.Count == 0)
            {
                break;
            }
        }

        for (var index = 0; index < parts.Count; index++)
        {
            var part = parts[index];
            if (part <= 0)
            {
                parts.RemoveAt(index);
                index++;
            }
            else
            {
                // break when there isn't a zero part
                break;
            }
        }

        if (parts.Count == 0)
        {
            throw new InvalidOperationException("Cannot subtract a revision from a zero version");
        }

        var lastNonZero = parts.FindLastIndex(i => i > 0);

        // subtract 1 from the last non-zero
        parts[lastNonZero] = parts[lastNonZero] - 1;

        // the last non zero is actually the revision so we can just return
        if (lastNonZero == parts.Count - 1)
        {
            return FromList(parts);
        }

        // the last non zero isn't the revision so the remaining zero's need to be replaced with int.max
        for (var i = lastNonZero + 1; i < parts.Count; i++)
        {
            parts[i] = int.MaxValue;
        }

        return FromList(parts);
    }

    private static Version FromList(IList<int> parts)
    {
        while (parts.Count < 4)
        {
            parts.Insert(0, 0);
        }

        return new Version(parts[0], parts[1], parts[2], parts[3]);
    }
}
