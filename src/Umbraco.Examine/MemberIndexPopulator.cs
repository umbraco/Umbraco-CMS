using System.Collections.Generic;
using System.Linq;
using Examine;
using Lucene.Net.Util;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{
    public class MemberIndexPopulator : IndexPopulator
    {
        private readonly IMemberService _memberService;
        private readonly IValueSetBuilder<IMember> _valueSetBuilder;

        public MemberIndexPopulator(IMemberService memberService, IValueSetBuilder<IMember> valueSetBuilder)
        {
            _memberService = memberService;
            _valueSetBuilder = valueSetBuilder;

            RegisterIndex(Core.Constants.UmbracoIndexes.MembersIndexName);
        }
        protected override void PopulateIndexes(IEnumerable<IIndexer> indexes)
        {
            const int pageSize = 1000;
            var pageIndex = 0;

            IMember[] members;

            //TODO: Add validators for member indexers for ConfigIndexCriteria.IncludeItemTypes

            //no node types specified, do all members
            do
            {
                members = _memberService.GetAll(pageIndex, pageSize, out _).ToArray();

                if (members.Length > 0)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var index in indexes)
                        index.IndexItems(_valueSetBuilder.GetValueSets(members));
                }

                pageIndex++;
            } while (members.Length == pageSize);
        }
    }
}
