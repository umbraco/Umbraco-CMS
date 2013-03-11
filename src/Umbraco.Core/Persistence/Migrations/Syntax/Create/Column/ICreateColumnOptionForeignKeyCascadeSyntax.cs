namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Column
{
    public interface ICreateColumnOptionForeignKeyCascadeSyntax : ICreateColumnOptionSyntax,
                                                                IForeignKeyCascadeSyntax<ICreateColumnOptionSyntax, ICreateColumnOptionForeignKeyCascadeSyntax>
    {
         
    }
}