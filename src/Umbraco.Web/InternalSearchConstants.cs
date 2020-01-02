using System.Collections.Generic;
using Umbraco.Examine;
using Umbraco.Web.Search;

namespace Umbraco.Web
{
    public class InternalSearchConstants : IInternalSearchConstants
    {
        private List<string> _backOfficeFields = new List<string> {"id", "__NodeId", "__Key"};
        public List<string> GetBackOfficeFields()
        {
            return _backOfficeFields;
        }


        private List<string> _backOfficeMembersFields = new List<string> {"email", "loginName"};
        public List<string> GetBackOfficeMembersFields()
        {
            return _backOfficeMembersFields;
        }
        private List<string> _backOfficeMediaFields = new List<string> {UmbracoExamineIndex.UmbracoFileFieldName };
        public List<string> GetBackOfficeMediaFields()
        {
            return _backOfficeMediaFields;
        }
        private List<string> _backOfficeDocumentFields = new List<string> ();
        public List<string> GetBackOfficeDocumentFields()
        {
            return _backOfficeDocumentFields;
        }
    }
}
