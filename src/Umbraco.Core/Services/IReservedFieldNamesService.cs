namespace Umbraco.Cms.Core.Services;

public interface IReservedFieldNamesService
{
    ISet<string> GetDocumentReservedFieldNames();

    ISet<string> GetMediaReservedFieldNames();

    ISet<string> GetMemberReservedFieldNames();
}
