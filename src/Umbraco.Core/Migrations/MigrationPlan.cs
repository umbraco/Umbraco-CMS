using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Type = System.Type;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents a migration plan.
    /// </summary>
    public class MigrationPlan
    {
        private readonly Dictionary<string, Transition> _transitions = new Dictionary<string, Transition>();

        private string _prevState;
        private string _finalState;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationPlan"/> class.
        /// </summary>
        /// <param name="name">The name of the plan.</param>
        public MigrationPlan(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;
        }

        /// <summary>
        /// Gets the transitions.
        /// </summary>
        public IReadOnlyDictionary<string, Transition> Transitions => _transitions;

        /// <summary>
        /// Gets the name of the plan.
        /// </summary>
        public string Name { get; }

        // adds a transition
        private MigrationPlan Add(string sourceState, string targetState, Type migration)
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
            _transitions[sourceState] = new Transition(sourceState, targetState, migration);

            // register the target state if we don't know it already
            // this is how we keep track of the final state - because
            // transitions could be defined in any order, that might
            // be overriden afterwards.
            if (!_transitions.ContainsKey(targetState))
                _transitions.Add(targetState, null);

            _prevState = targetState;
            _finalState = null; // force re-validation

            return this;
        }

        /// <summary>
    /// Adds a transition to a target state through an empty migration.
    /// </summary>
        public MigrationPlan To(string targetState)
            => To<NoopMigration>(targetState);

        /// <summary>
        /// Adds a transition to a target state through a migration.
        /// </summary>
        public MigrationPlan To<TMigration>(string targetState)
            where TMigration : IMigration
            => To(targetState, typeof(TMigration));

        /// <summary>
        /// Adds a transition to a target state through a migration.
        /// </summary>
        public MigrationPlan To(string targetState, Type migration)
            => Add(_prevState, targetState, migration);

        /// <summary>
        /// Sets the starting state.
        /// </summary>
        public MigrationPlan From(string sourceState)
        {
            _prevState = sourceState ?? throw new ArgumentNullException(nameof(sourceState));
            return this;
        }

        /// <summary>
        /// Adds transitions to a target state by copying transitions from a start state to an end state.
        /// </summary>
        public MigrationPlan To(string targetState, string startState, string endState)
        {
            if (string.IsNullOrWhiteSpace(startState)) throw new ArgumentNullOrEmptyException(nameof(startState));
            if (string.IsNullOrWhiteSpace(endState)) throw new ArgumentNullOrEmptyException(nameof(endState));
            if (string.IsNullOrWhiteSpace(targetState)) throw new ArgumentNullOrEmptyException(nameof(targetState));

            if (startState == endState) throw new ArgumentException("Start and end states cannot be identical.");

            startState = startState.Trim();
            endState = endState.Trim();
            targetState = targetState.Trim();

            var state = startState;
            var visited = new HashSet<string>();

            while (state != endState)
            {
                if (visited.Contains(state))
                    throw new InvalidOperationException("A loop was detected in the copied chain.");
                visited.Add(state);

                if (!_transitions.TryGetValue(state, out var transition))
                    throw new InvalidOperationException($"There is no transition from state \"{state}\".");

                var newTargetState = transition.TargetState == endState
                    ? targetState
                    : Guid.NewGuid().ToString("B").ToUpper();
                To(newTargetState, transition.MigrationType);
                state = transition.TargetState;
            }

            return this;
        }

        /// <summary>
        /// Gets the initial state.
        /// </summary>
        /// <remarks>The initial state is the state when the plan has never
        /// run. By default, it is the empty string, but plans may override
        /// it if they have other ways of determining where to start from.</remarks>
        public virtual string InitialState => string.Empty;

        /// <summary>
        /// Gets the final state.
        /// </summary>
        public string FinalState
        {
            get
            {
                // modifying the plan clears _finalState
                // Validate() either sets _finalState, or throws
                if (_finalState == null)
                    Validate();

                return _finalState;
            }
        }

        /// <summary>
        /// Validates the plan.
        /// </summary>
        /// <returns>The plan's final state.</returns>
        public void Validate()
        {
            if (_finalState != null)
                return;

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

            _finalState = finalState;
        }

        /// <summary>
        /// Executes the plan.
        /// </summary>
        /// <param name="scope">A scope.</param>
        /// <param name="fromState">The state to start execution at.</param>
        /// <param name="migrationBuilder">A migration builder.</param>
        /// <param name="logger">A logger.</param>
        /// <returns>The final state.</returns>
        /// <remarks>The plan executes within the scope, which must then be completed.</remarks>
        public string Execute(IScope scope, string fromState, IMigrationBuilder migrationBuilder, ILogger logger)
        {
            Validate();

            if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info<MigrationPlan>("Starting '{MigrationName}'...", Name);

            var origState = fromState ?? string.Empty;

            logger.Info<MigrationPlan>("At {OrigState}", string.IsNullOrWhiteSpace(origState) ? "origin": origState);

            if (!_transitions.TryGetValue(origState, out var transition))
                throw new Exception($"Unknown state \"{origState}\".");

            var context = new MigrationContext(scope.Database, logger);

            while (transition != null)
            {
                var migration = migrationBuilder.Build(transition.MigrationType, context);
                migration.Migrate();

                var nextState = transition.TargetState;
                origState = nextState;

                logger.Info<MigrationPlan>("At {OrigState}", origState);

                if (!_transitions.TryGetValue(origState, out transition))
                    throw new Exception($"Unknown state \"{origState}\".");
            }

            logger.Info<MigrationPlan>("Done (pending scope completion).");

            // safety check
            if (origState != _finalState)
                throw new Exception($"Internal error, reached state {origState} which is not final state {_finalState}");

            return origState;
        }

        /// <summary>
        /// Follows a path (for tests and debugging).
        /// </summary>
        /// <remarks>Does the same thing Execute does, but does not actually execute migrations.</remarks>
        internal string FollowPath(string fromState, string toState = null)
        {
            toState = toState.NullOrWhiteSpaceAsNull();

            Validate();

            var origState = fromState ?? string.Empty;

            if (!_transitions.TryGetValue(origState, out var transition))
                throw new Exception($"Unknown state \"{origState}\".");

            while (transition != null)
            {
                var nextState = transition.TargetState;
                origState = nextState;

                if (nextState == toState)
                {
                    transition = null;
                    continue;
                }

                if (!_transitions.TryGetValue(origState, out transition))
                    throw new Exception($"Unknown state \"{origState}\".");
            }

            // safety check
            if (origState != (toState ?? _finalState))
                throw new Exception($"Internal error, reached state {origState} which is not state {toState ?? _finalState}");

            return origState;
        }

        /// <summary>
        /// Represents a plan transition.
        /// </summary>
        public class Transition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Transition"/> class.
            /// </summary>
            public Transition(string sourceState, string targetState, Type migrationTtype)
            {
                SourceState = sourceState;
                TargetState = targetState;
                MigrationType = migrationTtype;
            }

            /// <summary>
            /// Gets the source state.
            /// </summary>
            public string SourceState { get; }

            /// <summary>
            /// Gets the target state.
            /// </summary>
            public string TargetState { get; }

            /// <summary>
            /// Gets the migration type.
            /// </summary>
            public Type MigrationType { get; }

            /// <inheritdoc />
            public override string ToString()
            {
                return MigrationType == typeof(NoopMigration)
                    ? $"{(SourceState == "" ? "<empty>" : SourceState)} --> {TargetState}"
                    : $"{SourceState} -- ({MigrationType.FullName}) --> {TargetState}";
            }
        }
    }
}
