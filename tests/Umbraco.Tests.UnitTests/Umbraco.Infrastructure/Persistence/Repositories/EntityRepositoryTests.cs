// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class EntityRepositoryTests : BaseUsingSqlSyntax
{
    [Test]
    public void Ordering_By_A_Node_Field_Uses_The_Column_Name_As_Declared()
    {
        var orderBy = GetOrderByClause(Ordering.By("SortOrder"));

        Assert.That(orderBy, Does.StartWith("ORDER BY [umbracoNode].[sortOrder] "));
    }

    private string GetOrderByClause(Ordering? ordering)
    {
        var database = new Mock<IUmbracoDatabase>();
        database.Setup(x => x.SqlContext).Returns(SqlContext);
        Sql? pageQuery = null;
        database.Setup(x => x.Page<It.IsAnyType>(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Sql>()))
            .Returns(new InvocationFunc(invocation =>
            {
                pageQuery = (Sql)invocation.Arguments[2];
                return CreateEmptyPage(invocation.Method.ReturnType);
            }));

        var scope = new Mock<IScope>();
        scope.Setup(x => x.Database).Returns(database.Object);
        scope.Setup(x => x.SqlContext).Returns(SqlContext);
        var repository = new EntityRepository(Mock.Of<IScopeAccessor>(x => x.AmbientScope == scope.Object), AppCaches.NoCache);

        repository.GetPagedResultsByQuery(
            SqlContext.Query<IUmbracoEntity>(),
            [Constants.ObjectTypes.Document],
            0,
            10,
            out _,
            null,
            ordering);

        Assert.That(pageQuery, Is.Not.Null);
        var sql = pageQuery!.SQL.NoCrLf();
        return sql[sql.LastIndexOf("ORDER BY", StringComparison.Ordinal)..];
    }

    private static object CreateEmptyPage(Type pageType)
    {
        var page = Activator.CreateInstance(pageType)!;
        Type itemType = pageType.GetGenericArguments()[0];
        pageType.GetProperty(nameof(Page<object>.Items))!.SetValue(page, Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)));
        return page;
    }
}
