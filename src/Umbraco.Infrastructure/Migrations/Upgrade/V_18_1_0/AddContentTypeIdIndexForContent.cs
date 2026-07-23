// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_1_0;

/// <summary>
/// Re-applies the <see cref="V_17_6_0.AddContentTypeIdIndexForContent"/> migration for sites upgrading
/// from an 18.0.x release.
/// </summary>
/// <remarks>
/// The original migration was added for 17.6 and merged up, so it sits earlier in the plan than the
/// final 18.0 migration state. Sites already on 18.0.x have moved past that point in the chain and would
/// therefore never run it. Running it again here ensures they also gain the index. The operation is
/// idempotent (guarded by <c>IndexExists</c>), so sites that reached it via the 17.6 path are unaffected.
/// </remarks>
public class AddContentTypeIdIndexForContent : V_17_6_0.AddContentTypeIdIndexForContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentTypeIdIndexForContent"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddContentTypeIdIndexForContent(IMigrationContext context)
        : base(context)
    {
    }
}
