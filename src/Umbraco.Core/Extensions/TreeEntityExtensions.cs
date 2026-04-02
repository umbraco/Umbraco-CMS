using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ITreeEntity"/>.
/// </summary>
public static class TreeEntityExtensions
{
    /// <summary>
    /// Gets the ancestor IDs of the tree entity.
    /// </summary>
    /// <param name="entity">The tree entity.</param>
    /// <returns>An array of ancestor IDs, excluding the root and the entity itself.</returns>
    public static int[] AncestorIds(this ITreeEntity entity)
    {
        string[] commaSeparatedValues = entity.Path.Split(Constants.CharArrays.Comma);
        if (commaSeparatedValues.Length < 2)
        {
            return [];
        }

        int[] ancestorIds = new int[commaSeparatedValues.Length - 2];
        for (int i = 1; i <= commaSeparatedValues.Length - 2; i++)
        {
            ancestorIds[i - 1] = int.Parse(commaSeparatedValues[i], CultureInfo.InvariantCulture);
        }

        return ancestorIds;
    }
}
