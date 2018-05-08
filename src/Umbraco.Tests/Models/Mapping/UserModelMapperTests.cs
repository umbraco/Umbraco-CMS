using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class UserModelMapperTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void Map_UserGroupSave_To_IUserGroup()
        {
            var userModelMapper = new UserModelMapper();
            Mapper.Initialize(configuration => userModelMapper.ConfigureMappings(configuration, ApplicationContext.Current));

            var userService = ApplicationContext.Services.UserService;
            IUserGroup userGroup = new UserGroup(0, "alias", "name", new List<string> { "c" }, "icon");
            userService.Save(userGroup);

            // userGroup.permissions is System.Collections.Generic.List`1[System.String]

            userGroup = userService.GetUserGroupById(userGroup.Id);

            // userGroup.permissions is System.Linq.Enumerable+WhereSelectArrayIterator`2[System.Char, System.String]
            // fixed: now System.Collections.Generic.List`1[System.String]

            const string json = "{\"id\":@@@ID@@@,\"alias\":\"perm1\",\"name\":\"Perm1\",\"icon\":\"icon-users\",\"sections\":[\"content\"],\"users\":[],\"defaultPermissions\":[\"F\",\"C\",\"A\"],\"assignedPermissions\":{},\"startContentId\":-1,\"startMediaId\":-1,\"action\":\"save\",\"parentId\":-1}";
            var userGroupSave = JsonConvert.DeserializeObject<UserGroupSave>(json.Replace("@@@ID@@@", userGroup.Id.ToString()));

            // failed, AutoMapper complained, "Unable to cast object of type 'WhereSelectArrayIterator`2[System.Char,System.String]' to type 'System.Collections.IList'".
            // fixmed: added ToList() in UserGroupFactory
            Mapper.Map(userGroupSave, userGroup);
        }
    }
}
