namespace Umbraco.Core.Migrations.Syntax.Alter.Column
{
    public interface IAlterColumnOptionForeignKeyCascadeSyntax :
        IAlterColumnOptionSyntax,
        IForeignKeyCascadeSyntax<IAlterColumnOptionSyntax, IAlterColumnOptionForeignKeyCascadeSyntax>
    {

    }
}
