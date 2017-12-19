using System.Data;

namespace Umbraco.Core.Migrations.Expressions.Create.ForeignKey
{
    public interface ICreateForeignKeyCascadeBuilder : IFluentBuilder
    {
        ICreateForeignKeyCascadeBuilder OnDelete(Rule rule);
        ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule);
        void OnDeleteOrUpdate(Rule rule);
    }
}
