using Examine;
using Examine.LuceneEngine.Providers;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;
using Umbraco.Examine;

namespace Umbraco.Web.Search
{
    public class UmbracoTreeSearcherFields : IUmbracoTreeSearcherFields2
    {
        private IReadOnlyList<string> _backOfficeFields = new List<string> {"id", LuceneIndex.ItemIdFieldName, UmbracoExamineIndex.NodeKeyFieldName};
        private readonly ISet<string> _backOfficeFieldsToLoad = new HashSet<string> { "id", LuceneIndex.ItemIdFieldName, UmbracoExamineIndex.NodeKeyFieldName, "nodeName", UmbracoExamineIndex.IconFieldName, LuceneIndex.CategoryFieldName, "parentID", LuceneIndex.ItemTypeFieldName };
        private IReadOnlyList<string> _backOfficeMediaFields = new List<string> { UmbracoExamineIndex.UmbracoFileFieldName };
        private readonly ISet<string> _backOfficeMediaFieldsToLoad = new HashSet<string> { UmbracoExamineIndex.UmbracoFileFieldName };
        private IReadOnlyList<string> _backOfficeMembersFields = new List<string> { "email", "loginName" };
        private readonly ISet<string> _backOfficeMembersFieldsToLoad = new HashSet<string> { "email", "loginName" };
        private readonly ISet<string> _backOfficeDocumentFieldsToLoad = new HashSet<string> { UmbracoContentIndex.VariesByCultureFieldName };
        private readonly ILocalizationService _localizationService;

        public UmbracoTreeSearcherFields(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetBackOfficeFields() => _backOfficeFields;

        /// <inheritdoc />
        public virtual IEnumerable<string> GetBackOfficeMembersFields() => _backOfficeMembersFields;

        /// <inheritdoc />
        public virtual IEnumerable<string> GetBackOfficeMediaFields() => _backOfficeMediaFields;

        /// <inheritdoc />
        public virtual IEnumerable<string> GetBackOfficeDocumentFields() => Enumerable.Empty<string>();

        /// <inheritdoc />
        public virtual ISet<string> GetBackOfficeFieldsToLoad() => _backOfficeFieldsToLoad;

        /// <inheritdoc />
        public virtual ISet<string> GetBackOfficeMembersFieldsToLoad() => _backOfficeMembersFieldsToLoad;

        /// <inheritdoc />
        public virtual ISet<string> GetBackOfficeMediaFieldsToLoad() => _backOfficeMediaFieldsToLoad;

        /// <inheritdoc />
        public virtual ISet<string> GetBackOfficeDocumentFieldsToLoad()
        {
            var fields = _backOfficeDocumentFieldsToLoad;

            // We need to load all nodeName_* fields but we won't know those up front so need to get
            // all langs (this is cached)
            foreach(var field in _localizationService.GetAllLanguages().Select(x => "nodeName_" + x.IsoCode.ToLowerInvariant()))
            {
                fields.Add(field);
            }

            return fields;
        }
    }
}
