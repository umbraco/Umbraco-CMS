using System.Collections.Generic;

namespace Umbraco.Web.Search
{
    public interface IInternalSearchConstants
    {
        List<string> GetBackOfficeFields();
        List<string> GetBackOfficeMembersFields();
        
        List<string> GetBackOfficeMediaFields();
        List<string> GetBackOfficeDocumentFields();
    }
}
