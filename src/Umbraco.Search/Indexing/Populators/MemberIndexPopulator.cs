using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Search.Indexing.Populators;

public class MemberIndexPopulator : IndexPopulator
{
    private readonly ISearchProvider _provider;
    private readonly IMemberService _memberService;

    public MemberIndexPopulator(IMemberService memberService, ISearchProvider provider)
    {
        _provider = provider;
        _memberService = memberService;
    }
    public override bool IsRegistered(string index)
    {
        if (base.IsRegistered(index))
        {
            return true;
        }

        var indexer = _provider.GetIndex(index);
        if (!(indexer is IUmbracoIndex<IMember> casted))
        {
            return false;
        }

        return true;
    }
    protected override void PopulateIndexes(IReadOnlyList<string> indexes)
    {
        if (indexes.Count == 0)
        {
            return;
        }

        const int pageSize = 1000;
        var pageIndex = 0;

        IMember[] members;

        // no node types specified, do all members
        do
        {
            members = _memberService.GetAll(pageIndex, pageSize, out _).ToArray();

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (string index in indexes)
            {
                _provider.GetIndex<IMember>(index)?.IndexItems(members);
            }

            pageIndex++;
        }
        while (members.Length == pageSize);
    }
}
