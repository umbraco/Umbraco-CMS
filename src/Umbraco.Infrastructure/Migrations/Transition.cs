
namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Represents a plantransition.
/// </summary>
public class Transition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Transition" /> class.
    /// </summary>
    public Transition(string sourceState, string targetState, Type migrationType)
    {
        SourceState = sourceState;
        TargetState = targetState;
        MigrationType = migrationType;
    }

    /// <summary>
    ///     Gets the source state.
    /// </summary>
    public string SourceState { get; }

    /// <summary>
    ///     Gets the target state.
    /// </summary>
    public string TargetState { get; }

    /// <summary>
    ///     Gets the migration type.
    /// </summary>
    public Type MigrationType { get; }

    /// <inheritdoc />
    public override string ToString() =>
        MigrationType == typeof(NoopMigration)
            ? $"{(SourceState == string.Empty ? "<empty>" : SourceState)} --> {TargetState}"
            : $"{SourceState} -- ({MigrationType.FullName}) --> {TargetState}";
}
