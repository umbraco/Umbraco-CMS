using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Migrations
{
    public class MigrationPlan
    {
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly ILogger _logger;
        private readonly Dictionary<string, Transition> _transitions = new Dictionary<string, Transition>();

        private string _prevState;
        private string _finalState;

        // initializes a non-executing plan
        public MigrationPlan(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;
        }

        // initializes a plan
        public MigrationPlan(string name, IMigrationBuilder migrationBuilder, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;
            _migrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Name { get; }

        public MigrationPlan Add(string sourceState, string targetState)
            => Add<NoopMigration>(sourceState, targetState);

        public MigrationPlan Add<TMigration>(string sourceState, string targetState)
            where TMigration : IMigration
            => Add(sourceState, targetState, typeof(TMigration));

        public MigrationPlan Add(string sourceState, string targetState, Type migration)
        {
            if (sourceState == null) throw new ArgumentNullException(nameof(sourceState));
            if (string.IsNullOrWhiteSpace(targetState)) throw new ArgumentNullOrEmptyException(nameof(targetState));
            if (sourceState == targetState) throw new ArgumentException("Source and target state cannot be identical.");
            if (migration == null) throw new ArgumentNullException(nameof(migration));
            if (!migration.Implements<IMigration>()) throw new ArgumentException($"Type {migration.Name} does not implement IMigration.", nameof(migration));

            sourceState = sourceState.Trim();
            targetState = targetState.Trim();

            // throw if we already have a transition for that state which is not null,
            // null is used to keep track of the last step of the chain
            if (_transitions.ContainsKey(sourceState) && _transitions[sourceState] != null)
                throw new InvalidOperationException($"A transition from state \"{sourceState}\" has already been defined.");

            // register the transition
            _transitions[sourceState] = new Transition
            {
                SourceState = sourceState,
                TargetState = targetState,
                MigrationType = migration
            };

            // register the target state if we don't know it already
            // this is how we keep track of the final state - because
            // transitions could be defined in any order, that might
            // be overriden afterwards.
            if (!_transitions.ContainsKey(targetState))
                _transitions.Add(targetState, null);

            _prevState = targetState;
            _finalState = null;

            return this;
        }

        public MigrationPlan Chain(string targetState)
            => Chain<NoopMigration>(targetState);

        public MigrationPlan Chain<TMigration>(string targetState)
            where TMigration : IMigration
            => Chain(targetState, typeof(TMigration));

        public MigrationPlan Chain(string targetState, Type migration)
            => Add(_prevState, targetState, migration);

        public MigrationPlan From(string sourceState)
        {
            _prevState = sourceState ?? throw new ArgumentNullException(nameof(sourceState));
            return this;
        }

        public virtual string InitialState => string.Empty;

        public string FinalState => _finalState ?? (_finalState = Validate());

        public string Validate()
        {
            // quick check for dead ends - a dead end is a transition that has a target state
            // that is not null and does not match any source state. such a target state has
            // been registered as a source state with a null transition. so there should be only
            // one.
            string finalState = null;
            foreach (var kvp in _transitions.Where(x => x.Value == null))
            {
                if (finalState == null)
                    finalState = kvp.Key;
                else
                    throw new Exception("Multiple final states have been detected.");
            }

            // now check for loops
            var verified = new List<string>();
            foreach (var transition in _transitions.Values)
            {
                if (transition == null || verified.Contains(transition.SourceState)) continue;

                var visited = new List<string> { transition.SourceState };
                var nextTransition = _transitions[transition.TargetState];
                while (nextTransition != null && !verified.Contains(nextTransition.SourceState))
                {
                    if (visited.Contains(nextTransition.SourceState))
                        throw new Exception("A loop has been detected.");
                    visited.Add(nextTransition.SourceState);
                    nextTransition = _transitions[nextTransition.TargetState];
                }
                verified.AddRange(visited);
            }

            return finalState;
        }

        public string Execute(IScope scope, string fromState)
        {
            Validate();

            if (_migrationBuilder == null || _logger == null)
                throw new InvalidOperationException("Cannot execute a non-executing plan.");

            _logger.Info<MigrationPlan>("Starting \"{0}\"...", () => Name);
            var origState = fromState; //GetState();
            var info = "At " + (string.IsNullOrWhiteSpace(origState) ? "origin" : ("\"" + origState + "\"")) + ".";
            info = info.Replace("{", "{{").Replace("}", "}}"); // stupid log4net
            _logger.Info<MigrationPlan>(info);

            if (!_transitions.TryGetValue(origState, out var transition))
                throw new Exception($"Unknown state \"{origState}\".");

            var context = new MigrationContext(scope.Database, _logger);

            while (transition != null)
            {
                var migration = _migrationBuilder.Build(transition.MigrationType, context);
                migration.Migrate();

                var nextState = transition.TargetState;
                origState = nextState;
                
                _logger.Info<MigrationPlan>("At \"{0}\".", origState);

                if (!_transitions.TryGetValue(origState, out transition))
                    throw new Exception($"Unknown state \"{origState}\".");
            }

            _logger.Info<MigrationPlan>("Done (pending scope completion).");

            // fixme - what about post-migrations?
            return origState;
        }

        private class Transition
        {
            public string SourceState { get; set; }
            public string TargetState { get; set; }
            public Type MigrationType { get; set; }
        }
    }
}
