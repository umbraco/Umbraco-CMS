namespace Umbraco.Cms.Search.Core.Extensions;

public static class GuidExtensions
{
    public static string AsKeyword(this Guid guid) => guid.ToString("D");
}
