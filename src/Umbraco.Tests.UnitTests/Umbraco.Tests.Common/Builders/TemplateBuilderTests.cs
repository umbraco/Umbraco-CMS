using System;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class TemplateBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const int id = 3;
            const string alias = "test";
            const string name = "Test";
            var key = Guid.NewGuid();
            var createDate = DateTime.Now.AddHours(-1);
            var updateDate = DateTime.Now;
            const string path = "-1,3";
            const string content = "blah";
            const string masterTemplateAlias = "master";
            const int masterTemplateId = 88;

            var builder = new TemplateBuilder();

            // Act
            var template = builder
                .WithId(3)
                .WithAlias(alias)
                .WithName(name)
                .WithCreateDate(createDate)
                .WithUpdateDate(updateDate)
                .WithKey(key)
                .WithPath(path)
                .WithContent(content)
                .AsMasterTemplate(masterTemplateAlias, masterTemplateId)
                .Build();

            // Assert
            Assert.AreEqual(id, template.Id);
            Assert.AreEqual(alias, template.Alias);
            Assert.AreEqual(name, template.Name);
            Assert.AreEqual(createDate, template.CreateDate);
            Assert.AreEqual(updateDate, template.UpdateDate);
            Assert.AreEqual(key, template.Key);
            Assert.AreEqual(path, template.Path);
            Assert.AreEqual(content, template.Content);
            Assert.IsTrue(template.IsMasterTemplate);
            Assert.AreEqual(masterTemplateAlias, template.MasterTemplateAlias);
            Assert.AreEqual(masterTemplateId, template.MasterTemplateId.Value);
        }
    }
}
