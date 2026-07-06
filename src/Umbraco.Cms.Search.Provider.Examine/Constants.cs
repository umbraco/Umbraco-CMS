namespace Umbraco.Cms.Search.Provider.Examine;

internal static class Constants
{
    public static class Api
    {
        public const string Name = "search-examine-provider";
    }

    public static class Provider
    {
        public const string Name = Api.Name;
    }

    public static class Variance
    {
        public const string Invariant = "none";
    }

    public static class FieldValues
    {
        public const string Integers = "integers";
        public const string Decimals = "decimals";
        public const string DateTimeOffsets = "datetimeoffsets";
        public const string Keywords = "keywords";
        public const string Texts = "texts";
        public const string TextsR1 = "textsr1";
        public const string TextsR2 = "textsr2";
        public const string TextsR3 = "textsr3";
    }

    public static class SystemFields
    {
        private const string Prefix = "Sys_";

        public const string Protection = $"{Prefix}Protection";
        public const string Culture = $"{Prefix}Culture";

        public const string AggregatedTexts = $"{Prefix}aggregated_texts";
        public const string AggregatedTextsR1 = $"{Prefix}aggregated_textsr1";
        public const string AggregatedTextsR2 = $"{Prefix}aggregated_textsr2";
        public const string AggregatedTextsR3 = $"{Prefix}aggregated_textsr3";
    }
}
