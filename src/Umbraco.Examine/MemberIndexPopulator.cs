using System.Collections.Generic;
using System.Linq;
using Examine;
using Lucene.Net.Util;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{
    public class MemberIndexPopulator : IndexPopulator<UmbracoMemberIndex>
    {
        private readonly IMemberService _memberService;
        private readonly IValueSetBuilder<IMember> _valueSetBuilder;

        public MemberIndexPopulator(IMemberService memberService, IValueSetBuilder<IMember> valueSetBuilder)
        {
            _memberService = memberService;
            _valueSetBuilder = valueSetBuilder;
        }
        protected override void PopulateIndexes(IEnumerable<IIndex> indexes)
        {
            const int pageSize = 1000;
            var pageIndex = 0;

            IMember[] members;

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
