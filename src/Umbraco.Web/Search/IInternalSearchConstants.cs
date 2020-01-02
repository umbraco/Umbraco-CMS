using System.Collections.Generic;

namespace Umbraco.Web.Search
{
    public interface IInternalSearchConstants
    {
        IEnumerable<string> GetBackOfficeFields();
        IEnumerable<string> GetBackOfficeMembersFields();

        IEnumerable<string> GetBackOfficeMediaFields();
        IEnumerable<string> GetBackOfficeDocumentFields();
    }
}
