using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DataTypeMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {

            // Act
            string column = new DataTypeMapper().Map(new SqlCeSyntaxProvider(), "Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Key_Property()
        {

            // Act
            string column = new DataTypeMapper().Map(new SqlCeSyntaxProvider(), "Key");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[uniqueId]"));
        }

        [Test]
        public void Can_Map_DatabaseType_Property()
        {

            // Act
            string column = new DataTypeMapper().Map(new SqlCeSyntaxProvider(), "DatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[dbType]"));
        }

        [Test]
        public void Can_Map_PropertyEditorAlias_Property()
        {

            // Act
            string column = new DataTypeMapper().Map(new SqlCeSyntaxProvider(), "EditorAlias");

            // Assert
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
        }
    }
}
