// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Builds <see cref="ValueSet"/> instances for external-only members so they can be indexed by Examine.
/// </summary>
/// <remarks>
///     External-only members do not have content properties or content types,
///     so this builder produces a fixed set of fields from the <see cref="ExternalMemberIdentity"/> model.
/// </remarks>
public class ExternalMemberValueSetBuilder : IValueSetBuilder<ExternalMemberIdentity>
{
    /// <inheritdoc />
    public IEnumerable<ValueSet> GetValueSets(params ExternalMemberIdentity[] members)
    {
        foreach (ExternalMemberIdentity member in members)
        {
            var values = new Dictionary<string, IEnumerable<object?>>
            {
                { UmbracoExamineFieldNames.NodeKeyFieldName, new object[] { member.Key } },
                { UmbracoExamineFieldNames.NodeNameFieldName, member.Name?.Yield() ?? Enumerable.Empty<string>() },
                { "loginName", member.UserName.Yield() },
                { "email", member.Email.Yield() },
                { "createDate", new object[] { member.CreateDate } },
                { "id", new object[] { member.Id } },
                { "isExternalOnly", "1".Yield() },
            };

            var vs = new ValueSet(member.Id.ToInvariantString(), IndexTypes.Member, "ExternalMember", values);

            yield return vs;
        }
    }
}
