namespace Umbraco.Cms.Search.Core.Helpers;

public interface IDateTimeOffsetConverter
{
    DateTimeOffset ToDateTimeOffset(DateTime dateTime);
}
