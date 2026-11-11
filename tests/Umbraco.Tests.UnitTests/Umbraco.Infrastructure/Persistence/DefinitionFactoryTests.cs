// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class DefinitionFactoryTests
{
    [Test]
    public void Index_Columns_Use_The_Declared_Column_Names()
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(IndexedDto), Mock.Of<ISqlSyntaxProvider>());

        IndexDefinition index = table.Indexes.Single();
        Assert.Multiple(() =>
        {
            Assert.That(index.Columns.Select(x => x.Name), Is.EqualTo(new[] { "parentId", "sortOrder" }));
            Assert.That(index.IncludeColumns.Select(x => x.Name), Is.EqualTo(new[] { "uniqueId", "text" }));
        });
    }

    [TableName("umbTestIndexed")]
    private sealed class IndexedDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("parentId")]
        [Index(IndexTypes.NonClustered, Name = "IX_umbTestIndexed_parentId", ForColumns = "PARENTID,sortOrder", IncludeColumns = "uniqueID,Text")]
        public int ParentId { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("uniqueId")]
        public Guid UniqueId { get; set; }

        [Column("text")]
        public string? Text { get; set; }
    }
}
