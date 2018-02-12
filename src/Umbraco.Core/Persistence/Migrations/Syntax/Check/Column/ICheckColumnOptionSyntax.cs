namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public interface ICheckColumnOptionSyntax : IFluentSyntax
    {
        bool Exists();
    }
}
