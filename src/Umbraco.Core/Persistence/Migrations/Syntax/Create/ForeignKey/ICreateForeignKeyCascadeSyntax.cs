using System.Data;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyCascadeSyntax : IFluentSyntax
    {
        ICreateForeignKeyCascadeSyntax OnDelete(Rule rule);
        ICreateForeignKeyCascadeSyntax OnUpdate(Rule rule);
        void OnDeleteOrUpdate(Rule rule);
    }
}