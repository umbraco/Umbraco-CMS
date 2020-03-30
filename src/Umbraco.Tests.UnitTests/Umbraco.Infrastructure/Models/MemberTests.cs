using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class MemberTests
    {
        private readonly MemberBuilder _builder = new MemberBuilder();

        private const int _testMemberTypeId = 99;
        private const string _testMemberTypeAlias = "memberType";
        private const string _testMemberTypeName = "Member Type";
        private const string _testMemberTypePropertyGroupName = "Content";
        private const int _testId = 6;
        private const string _testName = "Fred";
        private const string _testUsername = "fred";
        private const string _testRawPasswordValue = "raw pass";
        private const string _testEmail = "email@email.com";
        private const int _testCreatorId = 22;
        private const int _testLevel = 3;
        private const string _testPath = "-1, 4, 10";
        private const bool _testIsApproved = true;
        private const bool _testIsLockedOut = true;
        private const int _testSortOrder = 5;
        private const bool _testTrashed = false;
        private readonly Guid _testKey = Guid.NewGuid();
        private readonly DateTime _testCreateDate = DateTime.Now.AddHours(-1);
        private readonly DateTime _testUpdateDate = DateTime.Now;
        private readonly DateTime _testLastLockoutDate = DateTime.Now.AddHours(-2);
        private readonly DateTime _testLastLoginDate = DateTime.Now.AddHours(-3);
        private readonly DateTime _testLastPasswordChangeDate = DateTime.Now.AddHours(-4);
        private readonly (string alias, string name, string description, int sortOrder, int dataTypeId) _testPropertyType1 = ("title", "Title", string.Empty, 1, -88);
        private readonly (string alias, string name, string description, int sortOrder, int dataTypeId) _testPropertyType2 = ("bodyText", "Body Text", string.Empty, 2, -87);
        private readonly (string alias, string name, string description, int sortOrder, int dataTypeId) _testPropertyType3 = ("author", "Author", "Writer of the article", 1, -88);
        private readonly string[] _testGroups = new string[] { "group1", "group2" };
        private readonly KeyValuePair<string, object> _testPropertyData1 = new KeyValuePair<string, object>("title", "Name member");
        private readonly KeyValuePair<string, object> _testPropertyData2 = new KeyValuePair<string, object>("bodyText", "This is a subpage");
        private readonly KeyValuePair<string, object> _testPropertyData3 = new KeyValuePair<string, object>("author", "John Doe");
        private readonly KeyValuePair<string, object> _testAdditionalData1 = new KeyValuePair<string, object>("test1", 123);
        private readonly KeyValuePair<string, object> _testAdditionalData2 = new KeyValuePair<string, object>("test2", "hello");

        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            // Act
            var member = BuildMember();

            // Assert
            Assert.AreEqual(_testMemberTypeId, member.ContentTypeId);
            Assert.AreEqual(_testMemberTypeAlias, member.ContentType.Alias);
            Assert.AreEqual(_testMemberTypeName, member.ContentType.Name);
            Assert.AreEqual(_testId, member.Id);
            Assert.AreEqual(_testKey, member.Key);
            Assert.AreEqual(_testName, member.Name);
            Assert.AreEqual(_testCreateDate, member.CreateDate);
            Assert.AreEqual(_testUpdateDate, member.UpdateDate);
            Assert.AreEqual(_testCreatorId, member.CreatorId);
            Assert.AreEqual(_testGroups, member.Groups.ToArray());
            Assert.AreEqual(10, member.Properties.Count);   // 7 from membership properties group, 3 custom
            Assert.AreEqual(_testPropertyData1.Value, member.GetValue<string>(_testPropertyData1.Key));
            Assert.AreEqual(_testPropertyData2.Value, member.GetValue<string>(_testPropertyData2.Key));
            Assert.AreEqual(_testPropertyData3.Value, member.GetValue<string>(_testPropertyData3.Key));
            Assert.AreEqual(2, member.AdditionalData.Count);
            Assert.AreEqual(_testAdditionalData1.Value, member.AdditionalData[_testAdditionalData1.Key]);
            Assert.AreEqual(_testAdditionalData2.Value, member.AdditionalData[_testAdditionalData2.Key]);
        }

        [Test]
        public void Can_Deep_Clone()
        {
            // Arrange
            var member = BuildMember();

            // Act
            var clone = (Member)member.DeepClone();

            // Assert
            Assert.AreNotSame(clone, member);
            Assert.AreEqual(clone, member);
            Assert.AreEqual(clone.Id, member.Id);
            Assert.AreEqual(clone.VersionId, member.VersionId);
            Assert.AreEqual(clone.AdditionalData, member.AdditionalData);
            Assert.AreEqual(clone.ContentType, member.ContentType);
            Assert.AreEqual(clone.ContentTypeId, member.ContentTypeId);
            Assert.AreEqual(clone.CreateDate, member.CreateDate);
            Assert.AreEqual(clone.CreatorId, member.CreatorId);
            Assert.AreEqual(clone.Comments, member.Comments);
            Assert.AreEqual(clone.Key, member.Key);
            Assert.AreEqual(clone.FailedPasswordAttempts, member.FailedPasswordAttempts);
            Assert.AreEqual(clone.Level, member.Level);
            Assert.AreEqual(clone.Path, member.Path);
            Assert.AreEqual(clone.Groups, member.Groups);
            Assert.AreEqual(clone.Groups.Count(), member.Groups.Count());
            Assert.AreEqual(clone.IsApproved, member.IsApproved);
            Assert.AreEqual(clone.IsLockedOut, member.IsLockedOut);
            Assert.AreEqual(clone.SortOrder, member.SortOrder);
            Assert.AreEqual(clone.LastLockoutDate, member.LastLockoutDate);
            Assert.AreNotSame(clone.LastLoginDate, member.LastLoginDate);
            Assert.AreEqual(clone.LastPasswordChangeDate, member.LastPasswordChangeDate);
            Assert.AreEqual(clone.Trashed, member.Trashed);
            Assert.AreEqual(clone.UpdateDate, member.UpdateDate);
            Assert.AreEqual(clone.VersionId, member.VersionId);
            Assert.AreEqual(clone.RawPasswordValue, member.RawPasswordValue);
            Assert.AreNotSame(clone.Properties, member.Properties);
            Assert.AreEqual(clone.Properties.Count(), member.Properties.Count());
            for (var index = 0; index < member.Properties.Count; index++)
            {
                Assert.AreNotSame(clone.Properties[index], member.Properties[index]);
                Assert.AreEqual(clone.Properties[index], member.Properties[index]);
            }

            // this can be the same, it is immutable
            Assert.AreSame(clone.ContentType, member.ContentType);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(member, null));
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var member = BuildMember();

            var json = JsonConvert.SerializeObject(member);
            Debug.Print(json);
        }

        private Member BuildMember()
        {
            return _builder
                .AddMemberType()
                    .WithId(_testMemberTypeId)
                    .WithAlias(_testMemberTypeAlias)
                    .WithName(_testMemberTypeName)
                    .WithMembershipPropertyGroup()
                    .AddPropertyGroup()
                        .WithName(_testMemberTypePropertyGroupName)
                        .WithSortOrder(1)
                        .AddPropertyType()
                            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                            .WithValueStorageType(ValueStorageType.Nvarchar)
                            .WithAlias(_testPropertyType1.alias)
                            .WithName(_testPropertyType1.name)
                            .WithSortOrder(_testPropertyType1.sortOrder)
                            .WithDataTypeId(_testPropertyType1.dataTypeId)
                            .Done()
                        .AddPropertyType()
                            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                            .WithValueStorageType(ValueStorageType.Ntext)
                            .WithAlias(_testPropertyType2.alias)
                            .WithName(_testPropertyType2.name)
                            .WithSortOrder(_testPropertyType2.sortOrder)
                            .WithDataTypeId(_testPropertyType2.dataTypeId)
                            .Done()
                        .AddPropertyType()
                            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                            .WithValueStorageType(ValueStorageType.Nvarchar)
                            .WithAlias(_testPropertyType3.alias)
                            .WithName(_testPropertyType3.name)
                            .WithDescription(_testPropertyType3.description)
                            .WithSortOrder(_testPropertyType3.sortOrder)
                            .WithDataTypeId(_testPropertyType3.dataTypeId)
                            .Done()
                        .Done()
                    .Done()
                .WithId(_testId)
                .WithKey(_testKey)
                .WithName(_testName)
                .WithUserName(_testUsername)
                .WithRawPasswordValue(_testRawPasswordValue)
                .WithEmail(_testEmail)
                .WithCreatorId(_testCreatorId)
                .WithCreateDate(_testCreateDate)
                .WithUpdateDate(_testUpdateDate)
                .WithFailedPasswordAttempts(22)
                .WithLevel(_testLevel)
                .WithPath(_testPath)
                .WithIsApproved(_testIsApproved)
                .WithIsLockedOut(_testIsLockedOut)
                .WithLastLockoutDate(_testLastLockoutDate)
                .WithLastLoginDate(_testLastLoginDate)
                .WithLastPasswordChangeDate(_testLastPasswordChangeDate)                
                .WithSortOrder(_testSortOrder)
                .WithTrashed(_testTrashed)
                .AddMemberGroups()
                    .WithValue(_testGroups[0])
                    .WithValue(_testGroups[1])
                    .Done()
                .AddAdditionalData()
                    .WithKeyValue(_testAdditionalData1.Key, _testAdditionalData1.Value)
                    .WithKeyValue(_testAdditionalData2.Key, _testAdditionalData2.Value)
                    .Done()
                .WithPropertyIdsIncrementingFrom(200)
                .AddPropertyData()
                    .WithKeyValue(_testPropertyData1.Key, _testPropertyData1.Value)
                    .WithKeyValue(_testPropertyData2.Key, _testPropertyData2.Value)
                    .WithKeyValue(_testPropertyData3.Key, _testPropertyData3.Value)
                    .Done()
                .Build();
        }
    }
}
