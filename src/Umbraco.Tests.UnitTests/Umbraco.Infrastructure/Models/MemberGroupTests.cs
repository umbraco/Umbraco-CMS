using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class MemberGroupTests
    {
        private readonly MemberGroupBuilder _builder = new MemberGroupBuilder();

        private const int _testId = 6;
        private const string _testName = "Test Group";
        private const int _testCreatorId = 4;
        private readonly Guid _testKey = Guid.NewGuid();
        private readonly DateTime _testCreateDate = DateTime.Now.AddHours(-1);
        private readonly DateTime _testUpdateDate = DateTime.Now;
        private readonly KeyValuePair<string, object> _testAdditionalData1 = new KeyValuePair<string, object>("test1", 123);
        private readonly KeyValuePair<string, object> _testAdditionalData2 = new KeyValuePair<string, object>("test2", "hello");

        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            // Act
            var group = BuildMemberGroup();

            // Assert
            Assert.AreEqual(_testId, group.Id);
            Assert.AreEqual(_testKey, group.Key);
            Assert.AreEqual(_testName, group.Name);
            Assert.AreEqual(_testCreateDate, group.CreateDate);
            Assert.AreEqual(_testUpdateDate, group.UpdateDate);
            Assert.AreEqual(_testCreatorId, group.CreatorId);
            Assert.AreEqual(2, group.AdditionalData.Count);
            Assert.AreEqual(_testAdditionalData1.Value, group.AdditionalData[_testAdditionalData1.Key]);
            Assert.AreEqual(_testAdditionalData2.Value, group.AdditionalData[_testAdditionalData2.Key]);
        }

        [Test]
        public void Can_Deep_Clone()
        {
            // Arrange
            var group = BuildMemberGroup();

            // Act
            var clone = (MemberGroup)group.DeepClone();

            // Assert
            Assert.AreNotSame(clone, group);
            Assert.AreEqual(clone, group);
            Assert.AreEqual(clone.Id, group.Id);
            Assert.AreEqual(clone.AdditionalData, group.AdditionalData);
            Assert.AreEqual(clone.AdditionalData.Count, group.AdditionalData.Count);
            Assert.AreEqual(clone.CreateDate, group.CreateDate);
            Assert.AreEqual(clone.CreatorId, group.CreatorId);
            Assert.AreEqual(clone.Key, group.Key);
            Assert.AreEqual(clone.UpdateDate, group.UpdateDate);
            Assert.AreEqual(clone.Name, group.Name);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(group, null));
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var group = BuildMemberGroup();

            var json = JsonConvert.SerializeObject(group);
            Debug.Print(json);
        }

        private MemberGroup BuildMemberGroup()
        {
            return _builder
                .WithId(_testId)
                .WithKey(_testKey)
                .WithName(_testName)
                .WithCreatorId(_testCreatorId)
                .WithCreateDate(_testCreateDate)
                .WithUpdateDate(_testUpdateDate)
                .AddAdditionalData()
                    .WithKeyValue(_testAdditionalData1.Key, _testAdditionalData1.Value)
                    .WithKeyValue(_testAdditionalData2.Key, _testAdditionalData2.Value)
                    .Done()
                .Build();
        }
    }
}
