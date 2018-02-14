namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    /// <summary>
    /// The underlying interface that ensures an entity can evaluate it exists.
    /// </summary>
    public interface ICheckOptionSyntax : IFluentSyntax
    {
        /// <summary>
        /// Evalutes the current chain to see if it exists.
        /// </summary>
        /// <returns>Whether it exists or not.</returns>
        bool Exists();
    }
}
