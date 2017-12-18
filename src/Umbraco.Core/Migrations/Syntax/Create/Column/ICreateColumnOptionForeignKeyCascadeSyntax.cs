namespace Umbraco.Core.Migrations.Syntax.Create.Column
{
    public interface ICreateColumnOptionForeignKeyCascadeSyntax : ICreateColumnOptionSyntax,
                                                                IForeignKeyCascadeSyntax<ICreateColumnOptionSyntax, ICreateColumnOptionForeignKeyCascadeSyntax>
    {

    }
}
