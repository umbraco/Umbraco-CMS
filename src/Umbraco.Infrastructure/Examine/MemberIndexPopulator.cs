using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Populates and maintains the member index used by the Examine search engine in Umbraco.
/// This ensures that member data is searchable and kept up to date within the index.
/// </summary>
public class MemberIndexPopulator : IndexPopulator<IUmbracoMemberIndex>
{
    private readonly IMemberService _memberService;
    private readonly IValueSetBuilder<IMember> _valueSetBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.MemberIndexPopulator"/> class.
    /// </summary>
    /// <param name="memberService">Service for accessing and managing member data.</param>
    /// <param name="valueSetBuilder">Builder for creating value sets from member entities.</param>
    public MemberIndexPopulator(IMemberService memberService, IValueSetBuilder<IMember> valueSetBuilder)
    {
        _memberService = memberService;
        _valueSetBuilder = valueSetBuilder;
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
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
            foreach (IIndex index in indexes)
            {
                index.IndexItems(_valueSetBuilder.GetValueSets(members));
            }

            pageIndex++;
        }
        while (members.Length == pageSize);
    }
}
