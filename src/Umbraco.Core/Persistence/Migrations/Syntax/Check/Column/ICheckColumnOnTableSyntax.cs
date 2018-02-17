namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public interface ICheckColumnOnTableSyntax : ICheckExistsSyntax
    {
        bool IsNullable();
        bool IsNotNullable();

        bool IsDataType(string dataType);
    }
}
