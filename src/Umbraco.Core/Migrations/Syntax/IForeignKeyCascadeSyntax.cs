using System.Data;

namespace Umbraco.Core.Migrations.Syntax
{
    public interface IForeignKeyCascadeSyntax<TNext, TNextFk> : IFluentSyntax
        where TNext : IFluentSyntax
        where TNextFk : IFluentSyntax
    {
        TNextFk OnDelete(Rule rule);
        TNextFk OnUpdate(Rule rule);
        TNext OnDeleteOrUpdate(Rule rule);
    }
}
