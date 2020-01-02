using System.Collections.Generic;
using Umbraco.Examine;
using Umbraco.Web.Search;

namespace Umbraco.Web
{
    public class UmbracoTreeSearcherFields : IUmbracoTreeSearcherFields
    {
        private IReadOnlyList<string> _backOfficeFields = new List<string> {"id", "__NodeId", "__Key"};
        public IEnumerable<string> GetBackOfficeFields()
        {
            return _backOfficeFields;
        }


        private IReadOnlyList<string> _backOfficeMembersFields = new List<string> {"email", "loginName"};
        public IEnumerable<string> GetBackOfficeMembersFields()
        {
            return _backOfficeMembersFields;
        }
        private IReadOnlyList<string> _backOfficeMediaFields = new List<string> {UmbracoExamineIndex.UmbracoFileFieldName };
        public IEnumerable<string> GetBackOfficeMediaFields()
        {
            return _backOfficeMediaFields;
        }
        private IReadOnlyList<string> _backOfficeDocumentFields = new List<string> ();
        public IEnumerable<string> GetBackOfficeDocumentFields()
        {
            return _backOfficeDocumentFields;
        }
    }
}
