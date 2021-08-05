// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Infrastructure.Migrations.Test
{
    /// <summary>
    /// Defines the migration plan for the forms package database tables.
    /// </summary>
    public class TestPlan : MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormsMigrationPlan"/> class.
        /// </summary>
        public TestPlan()
            : base(Core.Constants.Conventions.Migrations.TestPlanName) => DefinePlan();

        /// <inheritdoc/>
        public override string InitialState => "{test-init-state}";

        /// <inheritdoc/>
        public override string ConnectionStringAlias => "Test";

        /// <summary>
        /// Defines the plan.
        /// </summary>
        protected void DefinePlan() =>
            From("{test-init-state}")
                .To<AddTestDto>("{9225dc6d-b422-491a-9200-ff12baac2e28}");
    }
}
