using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class UserModelMapperTests : TestWithDatabaseBase
    {
        [Test]
        public void Map_UserGroupSave_To_IUserGroup()
        {
            IUserGroup userGroup = new UserGroup(0, "alias", "name", new List<string> { "c" }, "icon");
            userGroup.Id = 42;

            // userGroup.permissions is System.Collections.Generic.List`1[System.String]

            // userGroup.permissions is System.Linq.Enumerable+WhereSelectArrayIterator`2[System.Char, System.String]
            // fixed: now System.Collections.Generic.List`1[System.String]

            const string json = "{\"id\":@@@ID@@@,\"alias\":\"perm1\",\"name\":\"Perm1\",\"icon\":\"icon-users\",\"sections\":[\"content\"],\"users\":[],\"defaultPermissions\":[\"F\",\"C\",\"A\"],\"assignedPermissions\":{},\"startContentId\":-1,\"startMediaId\":-1,\"action\":\"save\",\"parentId\":-1}";
            var userGroupSave = JsonConvert.DeserializeObject<UserGroupSave>(json.Replace("@@@ID@@@", userGroup.Id.ToString()));

            // failed, AutoMapper complained, "Unable to cast object of type 'WhereSelectArrayIterator`2[System.Char,System.String]' to type 'System.Collections.IList'".
            // FIXME: added ToList() in UserGroupFactory
            Mapper.Map(userGroupSave, userGroup);
        }
    }
}
