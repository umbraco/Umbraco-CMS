using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Examine
{
    public class MemberIndexPopulator: IIndexPopulator
    {
        private readonly IMemberService _memberService;
        private readonly IValueSetBuilder<IMember> _valueSetBuilder;

        public MemberIndexPopulator(IMemberService memberService, IValueSetBuilder<IMember> valueSetBuilder)
        {
            _memberService = memberService;
            _valueSetBuilder = valueSetBuilder;
        }
        public void Populate(params IIndexer[] indexes)
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
                    foreach (var index in indexes)
                        index.IndexItems(_valueSetBuilder.GetValueSets(members));
                }   
                
                pageIndex++;
            } while (members.Length == pageSize);
        }
    }
}
