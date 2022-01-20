using System.Collections.Generic;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class MemberValueSetValidator : ValueSetValidator
    {
        public MemberValueSetValidator() : base(null, null, DefaultMemberIndexFields, null)
        {
        }

        public MemberValueSetValidator(IEnumerable<string> includeItemTypes, IEnumerable<string> excludeItemTypes)
            : base(includeItemTypes, excludeItemTypes, DefaultMemberIndexFields, null)
        {
        }

        public MemberValueSetValidator(IEnumerable<string> includeItemTypes, IEnumerable<string> excludeItemTypes, IEnumerable<string> includeFields, IEnumerable<string> excludeFields)
            : base(includeItemTypes, excludeItemTypes, includeFields, excludeFields)
        {
        }

        /// <summary>
        /// By default these are the member fields we index
        /// </summary>
        public static readonly string[] DefaultMemberIndexFields = { "id", UmbracoExamineFieldNames.NodeNameFieldName, "updateDate", "loginName", "email", UmbracoExamineFieldNames.NodeKeyFieldName };

        private static readonly IEnumerable<string> ValidCategories = new[] { IndexTypes.Member };
        protected override IEnumerable<string> ValidIndexCategories => ValidCategories;

    }
}
