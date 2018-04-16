using System.Data;
using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.ForeignKey
{
    public interface ICreateForeignKeyCascadeBuilder : IFluentBuilder, IExecutableBuilder
    {
        ICreateForeignKeyCascadeBuilder OnDelete(Rule rule);
        ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule);
        IExecutableBuilder OnDeleteOrUpdate(Rule rule);
    }
}
