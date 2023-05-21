using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

public interface ICreateForeignKeyCascadeBuilder : IFluentBuilder, IExecutableBuilder
{
    ICreateForeignKeyCascadeBuilder OnDelete(Rule rule);

    ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule);

    IExecutableBuilder OnDeleteOrUpdate(Rule rule);
}
