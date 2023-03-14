// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Mapping;

[TestFixture]
[UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserModelMapperTests : UmbracoIntegrationTest
{
    [SetUp]
    public void Setup() => _sut = Services.GetRequiredService<IUmbracoMapper>();

    private IUmbracoMapper _sut;

    [Test]
    public void Map_UserGroupSave_To_IUserGroup()
    {
        IUserGroup userGroup =
            new UserGroup(ShortStringHelper, 0, "alias", "name", new List<string> { "c" }, "icon") { Id = 42 };

        // userGroup.permissions is List`1[System.String]

        // userGroup.permissions is System.Linq.Enumerable+WhereSelectArrayIterator`2[System.Char, System.String]
        // fixed: now List`1[System.String]
        const string json =
            "{\"id\":@@@ID@@@,\"alias\":\"perm1\",\"name\":\"Perm1\",\"icon\":\"icon-users\",\"sections\":[\"content\"],\"users\":[],\"defaultPermissions\":[\"F\",\"C\",\"A\"],\"assignedPermissions\":{},\"startContentId\":-1,\"startMediaId\":-1,\"action\":\"save\",\"parentId\":-1}";
        var userGroupSave =
            JsonConvert.DeserializeObject<UserGroupSave>(json.Replace("@@@ID@@@", userGroup.Id.ToString()));

        // failed, AutoMapper complained, "Unable to cast object of type 'WhereSelectArrayIterator`2[System.Char,System.String]' to type 'System.Collections.IList'".
        // FIXME: added ToList() in UserGroupFactory
        _sut.Map(userGroupSave, userGroup);
    }
}
