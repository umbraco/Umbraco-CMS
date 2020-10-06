using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Web.Actions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class FilterAllowedOutgoingContentAttributeTests
    {
        [Test]
        public void GetValueFromResponse_Already_EnumerableContent()
        {
            var expected = new List<ContentItemBasic>() {new ContentItemBasic()};

            var att = new FilterAllowedOutgoingContentFilter(expected.GetType(),
                null,
                ActionBrowse.ActionLetter,
                Mock.Of<IUserService>(),
                Mock.Of<IEntityService>(),
                Mock.Of<IBackofficeSecurityAccessor>() );

            var result = att.GetValueFromResponse(new ObjectResult(expected));

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetValueFromResponse_From_Property()
        {
            var expected = new List<ContentItemBasic>() { new ContentItemBasic() };
            var container = new MyTestClass() {MyList = expected};

            var att = new FilterAllowedOutgoingContentFilter(expected.GetType(),
                nameof(MyTestClass.MyList),
                ActionBrowse.ActionLetter,
                Mock.Of<IUserService>(),
                Mock.Of<IEntityService>(),
                Mock.Of<IBackofficeSecurityAccessor>() );

            var result = att.GetValueFromResponse(new ObjectResult(container));

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetValueFromResponse_Returns_Null_Not_Found_Property()
        {
            var expected = new List<ContentItemBasic>() { new ContentItemBasic() };
            var container = new MyTestClass() { MyList = expected };

            var att = new FilterAllowedOutgoingContentFilter(expected.GetType(),
                "DontFind",
                ActionBrowse.ActionLetter,
                Mock.Of<IUserService>(),
                Mock.Of<IEntityService>(),
                Mock.Of<IBackofficeSecurityAccessor>() );

            var actual = att.GetValueFromResponse(new ObjectResult(container));

            Assert.IsNull(actual);

        }

        [Test]
        public void Filter_On_Start_Node()
        {
            var user = CreateUser(id: 9, startContentId: 5);
            var userServiceMock = new Mock<IUserService>();
            var userService = userServiceMock.Object;
            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[] { Mock.Of<TreeEntityPath>(entity => entity.Id == 5 && entity.Path == "-1,5") });
            var entityService = entityServiceMock.Object;

            var list = new List<ContentItemBasic>();
            var att = new FilterAllowedOutgoingContentFilter(list.GetType(),
                null,
                ActionBrowse.ActionLetter,
                userService,
                entityService,
                Mock.Of<IBackofficeSecurityAccessor>() );

            var path = "";
            for (var i = 0; i < 10; i++)
            {
                if (i > 0 && path.EndsWith(",") == false)
                {
                    path += ",";
                }
                path += i.ToInvariantString();
                list.Add(new ContentItemBasic { Id = i, Name = "Test" + i, ParentId = i, Path = path });
            }

            att.FilterBasedOnStartNode(list, user);

            Assert.AreEqual(5, list.Count);

       }

        [Test]
        public void Filter_On_Permissions()
        {
            var list = new List<ContentItemBasic>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(new ContentItemBasic{Id = i, Name = "Test" + i, ParentId = -1});
            }
            var ids = list.Select(x => (int)x.Id).ToArray();

            var user = CreateUser(id: 9, startContentId: 0);

            var userServiceMock = new Mock<IUserService>();
            //we're only assigning 3 nodes browse permissions so that is what we expect as a result
            var permissions = new EntityPermissionCollection
            {
                new EntityPermission(9876, 1, new string[]{ ActionBrowse.ActionLetter.ToString() }),
                new EntityPermission(9876, 2, new string[]{ ActionBrowse.ActionLetter.ToString() }),
                new EntityPermission(9876, 3, new string[]{ ActionBrowse.ActionLetter.ToString() }),
                new EntityPermission(9876, 4, new string[]{ ActionUpdate.ActionLetter.ToString() })
            };
            userServiceMock.Setup(x => x.GetPermissions(user, ids)).Returns(permissions);
            var userService = userServiceMock.Object;

            var att = new FilterAllowedOutgoingContentFilter(list.GetType(),
                null,
                ActionBrowse.ActionLetter,
                userService,
                Mock.Of<IEntityService>(),
                Mock.Of<IBackofficeSecurityAccessor>() );
            att.FilterBasedOnPermissions(list, user);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list.ElementAt(0).Id);
            Assert.AreEqual(2, list.ElementAt(1).Id);
            Assert.AreEqual(3, list.ElementAt(2).Id);
        }

        private IUser CreateUser(int id = 0, int startContentId = -1)
        {
            return new UserBuilder()
                .WithId(id)
                .WithStartContentIds(new[] { startContentId })
                .AddUserGroup()
                    .WithId(1)
                    .WithName("admin")
                    .WithAlias("admin")
                    .Done()
                .Build();
        }

        private class MyTestClass
        {
            public IEnumerable<ContentItemBasic> MyList { get; set; }
        }
    }
}
