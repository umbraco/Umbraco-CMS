namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Column
{
    public interface ICheckColumnOnTableSyntax : ICheckOptionSyntax
    {
        bool IsNullable();
        bool IsNotNullable();

        bool IsDataType(string dataType);
    }
}
