// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Populates and maintains the member index with external-only member data
///     used by the Examine search engine in Umbraco.
/// </summary>
/// <remarks>
///     This populator pages through all external members via <see cref="IExternalMemberService"/>
///     and indexes them into any registered <see cref="IUmbracoMemberIndex"/>.
/// </remarks>
public class ExternalMemberIndexPopulator : IndexPopulator<IUmbracoMemberIndex>
{
    private readonly IExternalMemberService _externalMemberService;
    private readonly IValueSetBuilder<ExternalMemberIdentity> _valueSetBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberIndexPopulator"/> class.
    /// </summary>
    /// <param name="externalMemberService">Service for accessing external-only member data.</param>
    /// <param name="valueSetBuilder">Builder for creating value sets from external member entities.</param>
    public ExternalMemberIndexPopulator(
        IExternalMemberService externalMemberService,
        IValueSetBuilder<ExternalMemberIdentity> valueSetBuilder)
    {
        _externalMemberService = externalMemberService;
        _valueSetBuilder = valueSetBuilder;
    }

    /// <inheritdoc />
    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Count == 0)
        {
            return;
        }

        const int pageSize = 1000;
        var skip = 0;

        ExternalMemberIdentity[] members;

        do
        {
            members = _externalMemberService.GetAllAsync(skip, pageSize)
                .GetAwaiter().GetResult()
                .Items.ToArray();

            foreach (IIndex index in indexes)
            {
                index.IndexItems(_valueSetBuilder.GetValueSets(members));
            }

            skip += pageSize;
        }
        while (members.Length == pageSize);
    }
}
