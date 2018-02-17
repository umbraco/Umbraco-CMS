namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeyForToTableSyntax : ICheckExistsSyntax, ICheckForeignKeyTableSyntax<ICheckExistsSyntax>
    {
    }
}
