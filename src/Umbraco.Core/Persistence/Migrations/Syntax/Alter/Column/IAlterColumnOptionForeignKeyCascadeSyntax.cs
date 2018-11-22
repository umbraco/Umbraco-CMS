namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column
{
    public interface IAlterColumnOptionForeignKeyCascadeSyntax : 
        IAlterColumnOptionSyntax,
        IForeignKeyCascadeSyntax<IAlterColumnOptionSyntax, IAlterColumnOptionForeignKeyCascadeSyntax>
    {
         
    }
}