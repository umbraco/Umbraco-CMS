using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Extensions;

public static class TreeEntityExtensions
{
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
