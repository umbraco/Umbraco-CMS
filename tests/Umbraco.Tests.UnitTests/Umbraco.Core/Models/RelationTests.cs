// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class RelationTests
    {
        private RelationBuilder _builder;

        [SetUp]
        public void SetUp() => _builder = new RelationBuilder();

        [Test]
        public void Can_Deep_Clone()
        {
            Relation relation = BuildRelation();

            var clone = (Relation)relation.DeepClone();

            Assert.AreNotSame(clone, relation);
            Assert.AreEqual(clone, relation);
            Assert.AreEqual(clone.ChildId, relation.ChildId);
            Assert.AreEqual(clone.Comment, relation.Comment);
            Assert.AreEqual(clone.CreateDate, relation.CreateDate);
            Assert.AreEqual(clone.Id, relation.Id);
            Assert.AreEqual(clone.Key, relation.Key);
            Assert.AreEqual(clone.ParentId, relation.ParentId);
            Assert.AreNotSame(clone.RelationType, relation.RelationType);
            Assert.AreEqual(clone.RelationType, relation.RelationType);
            Assert.AreEqual(clone.RelationTypeId, relation.RelationTypeId);
            Assert.AreEqual(clone.UpdateDate, relation.UpdateDate);

            // This double verifies by reflection
            PropertyInfo[] allProps = clone.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(relation, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            Relation relation = BuildRelation();

            var json = JsonConvert.SerializeObject(relation);
            Debug.Print(json);
        }

        private Relation BuildRelation() =>
            _builder
                .BetweenIds(9, 8)
                .WithId(4)
                .WithComment("test comment")
                .WithCreateDate(DateTime.Now)
                .WithUpdateDate(DateTime.Now)
                .WithKey(Guid.NewGuid())
                .AddRelationType()
                    .WithId(66)
                    .WithAlias("test")
                    .WithName("Test")
                    .WithIsBidirectional(false)
                    .WithParentObjectType(Guid.NewGuid())
                    .WithChildObjectType(Guid.NewGuid())
                    .Done()
                .Build();
    }
}
