namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public interface ICheckIndexOptionSyntax : ICheckOptionSyntax
    {
        ICheckOptionSyntax Unique();
        ICheckOptionSyntax NotUnique();
    }
}
