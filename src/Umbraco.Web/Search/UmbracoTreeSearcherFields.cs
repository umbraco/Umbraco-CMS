using System.Collections.Generic;
using System.Linq;
using Umbraco.Examine;

namespace Umbraco.Web.Search
{
    public class UmbracoTreeSearcherFields : IUmbracoTreeSearcherFields2
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
        public IEnumerable<string> GetBackOfficeDocumentFields()
        {
            return Enumerable.Empty<string>();
        }

        private readonly ISet<string> _backOfficeFieldsToLoad = new HashSet<string> { "id", "__NodeId", "__Key" };
        public ISet<string> GetBackOfficeFieldsToLoad()
        {
            return _backOfficeFieldsToLoad;
        }

        private readonly ISet<string> _backOfficeMembersFieldsToLoad = new HashSet<string> { "id", "__NodeId", "__Key", "email", "loginName" };
        public ISet<string> GetBackOfficeMembersFieldsToLoad()
        {
            return _backOfficeMembersFieldsToLoad;
        }

        private readonly ISet<string> _backOfficeMediaFieldsToLoad = new HashSet<string> { "id", "__NodeId", "__Key", UmbracoExamineIndex.UmbracoFileFieldName };
        public ISet<string> GetBackOfficeMediaFieldsToLoad()
        {
            return _backOfficeMediaFieldsToLoad;
        }
        private readonly ISet<string> _backOfficeDocumentFieldsToLoad = new HashSet<string> { "id", "__NodeId", "__Key" };

        public ISet<string> GetBackOfficeDocumentFieldsToLoad()
        {
            return _backOfficeDocumentFieldsToLoad;
        }
    }
}
