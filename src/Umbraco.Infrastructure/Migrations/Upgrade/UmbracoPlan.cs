using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade
{
    /// <summary>
    ///     Represents the Umbraco CMS migration plan.
    /// </summary>
    /// <seealso cref="MigrationPlan" />
    public class UmbracoPlan : MigrationPlan
    {
        private const string InitPrefix = "{init-";
        private const string InitSuffix = "}";

        private readonly IUmbracoVersion _umbracoVersion;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UmbracoPlan" /> class.
        /// </summary>
        /// <param name="umbracoVersion">The Umbraco version.</param>
        public UmbracoPlan(IUmbracoVersion umbracoVersion)
            : base(Constants.Conventions.Migrations.UmbracoUpgradePlanName)
        {
            _umbracoVersion = umbracoVersion;
            DefinePlan();
        }

        /// <inheritdoc />
        /// <remarks>
        ///     <para>The default initial state in plans is string.Empty.</para>
        ///     <para>
        ///         When upgrading from version 9.5.0, we want to use specific initial states
        ///         that are e.g. "{init-9.5.0}", "{init-9.5.1}", etc. so we can chain the proper
        ///         migrations.
        ///     </para>
        ///     <para>
        ///         This is also where we detect the current version, and reject invalid
        ///         upgrades (from a tool old version, or going back in time, etc).
        ///     </para>
        /// </remarks>
        public override string InitialState
        {
            get
            {
                SemVersion currentVersion = _umbracoVersion.SemanticVersion;

                var minVersion = new SemVersion(9, 5);
                if (currentVersion < minVersion)
                {
                    throw new InvalidOperationException(
                        $"Version {currentVersion} cannot be migrated to {_umbracoVersion.SemanticVersion}."
                        + $" Please upgrade first to at least {minVersion}.");
                }

                // initial state is eg "{init-9.0.0}"
                return GetInitState(currentVersion);
            }
        }

        /// <summary>
        ///     Gets the initial state corresponding to a version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>
        ///     The initial state.
        /// </returns>
        private static string GetInitState(SemVersion version) => InitPrefix + version + InitSuffix;

        /// <summary>
        ///     Tries to extract a version from an initial state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="version">The version.</param>
        /// <returns>
        ///     <c>true</c> when the state contains a version; otherwise, <c>false</c>.D
        /// </returns>
        private static bool TryGetInitStateVersion(string state, out string version)
        {
            if (state.StartsWith(InitPrefix) && state.EndsWith(InitSuffix))
            {
                version = state.TrimStart(InitPrefix).TrimEnd(InitSuffix);
                return true;
            }

            version = null;
            return false;
        }

        /// <inheritdoc />
        public override void ThrowOnUnknownInitialState(string state)
        {
            if (TryGetInitStateVersion(state, out var initVersion))
            {
                throw new InvalidOperationException(
                    $"Version {_umbracoVersion.SemanticVersion} does not support migrating from {initVersion}."
                    + $" Please verify which versions support migrating from {initVersion}.");
            }

            base.ThrowOnUnknownInitialState(state);
        }

        /// <summary>
        ///     Defines the plan.
        /// </summary>
        protected void DefinePlan()
        {
            // MODIFYING THE PLAN
            //
            // Please take great care when modifying the plan!
            //
            // * Creating a migration for version 10:
            //     Append the migration to the main chain, using a new guid, before the "//FINAL" comment
            //
            //     If the new migration causes a merge conflict, because someone else also added another
            //     new migration, you NEED to fix the conflict by providing one default path, and paths
            //     out of the conflict states (see examples below).
            //
            // * Porting from version 9:
            //     Append the ported migration to the main chain, using a new guid (same as above).
            //     Create a new special chain from the {init-...} state to the main chain.

            // Plan starts at 9.5.0 (anything before 9.5.0 is not supported)
            From(GetInitState(new SemVersion(9, 5)));
        }
    }
}
