using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Extensions;

public static class TreeEntityExtensions
{
    public static int[] AncestorIds(this ITreeEntity entity) => entity.Path
        .Split(Constants.CharArrays.Comma)
        .Select(item => int.Parse(item, CultureInfo.InvariantCulture))
        .Take(new Range(Index.FromStart(1), Index.FromEnd(1)))
        .ToArray();
}
