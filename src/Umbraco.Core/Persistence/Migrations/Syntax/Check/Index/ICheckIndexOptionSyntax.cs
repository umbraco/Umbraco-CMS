namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public interface ICheckIndexOptionSyntax : ICheckExistsSyntax
    {
        ICheckExistsSyntax Unique();
        ICheckExistsSyntax NotUnique();
    }
}
