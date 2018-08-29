using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.ControllerTesting;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web._Legacy.Actions;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class ContentControllerTests : TestWithDatabaseBase
    {
        protected override void ComposeApplication(bool withApplication)
        {
            base.ComposeApplication(withApplication);

            //Replace with mockable services:

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(service => service.GetUserById(It.IsAny<int>()))
                .Returns((int id) => id == 1234 ? new User(1234, "Test", "test@test.com", "test@test.com", "", new List<IReadOnlyUserGroup>(), new int[0], new int[0]) : null);
            userServiceMock.Setup(service => service.GetPermissionsForPath(It.IsAny<IUser>(), It.IsAny<string>()))
                .Returns(new EntityPermissionSet(123, new EntityPermissionCollection(new[]
                {
                    new EntityPermission(0, 123, new[]
                    {
                        ActionBrowse.Instance.Letter.ToString(),
                        ActionUpdate.Instance.Letter.ToString(),
                        ActionPublish.Instance.Letter.ToString(),
                        ActionNew.Instance.Letter.ToString()
                    }),
                })));

            var entityService = new Mock<IEntityService>();
            entityService.Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
                .Returns((UmbracoObjectTypes objType, int[] ids) => ids.Select(x => new TreeEntityPath {Path = $"-1,{x}", Id = x}).ToList());

            var dataTypeService = new Mock<IDataTypeService>();
            dataTypeService.Setup(service => service.GetDataType(It.IsAny<int>()))
                .Returns(MockedDataType());

            Container.RegisterSingleton(f => Mock.Of<IContentService>());
            Container.RegisterSingleton(f => userServiceMock.Object);
            Container.RegisterSingleton(f => entityService.Object);
            Container.RegisterSingleton(f => dataTypeService.Object);
        }

        private IDataType MockedDataType()
        {
            return Mock.Of<IDataType>(type => type.Id == 9876 && type.Name == "text");
        }
        
        private MultipartFormDataContent GetMultiPartRequestContent(string json)
        {
            var multiPartBoundary = "----WebKitFormBoundary123456789";
            return new MultipartFormDataContent(multiPartBoundary)
            {
                new StringContent(json)
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "contentItem"
                        }
                    }
                }
            };
        }

        private const string PublishJson1 = @"{
    ""id"": 123,
    ""contentTypeAlias"": ""page"",
    ""parentId"": -1,
    ""action"": ""save"",
    ""variants"": [
        {
            ""name"": null,
            ""properties"": [
                {
                    ""id"": 1,
                    ""alias"": ""title"",
                    ""value"": ""asdf""
                }
            ],
            ""culture"": ""en-US""
        },
        {
            ""name"": null,
            ""properties"": [
                {
                    ""id"": 1,
                    ""alias"": ""title"",
                    ""value"": ""asdf""
                }
            ],
            ""culture"": ""fr-FR""
        },
        {
            ""name"": ""asdf"",
            ""properties"": [
                {
                    ""id"": 1,
                    ""alias"": ""title"",
                    ""value"": ""asdf""
                }
            ],
            ""culture"": ""es-ES"",
            ""save"": true,
            ""publish"": true
        }
    ]
}";

        /// <summary>
        /// Returns 404 if the content wasn't found based on the ID specified
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task PostSave_Validate_Existing_Content()
        {
            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                //var content = MockedContent.CreateSimpleContent(MockedContentTypes.CreateSimpleContentType());
                //content.Id = 999999999; //this will not be found
                //content.Path = "-1,999999999";

                var contentServiceMock = Mock.Get(Current.Services.ContentService);
                contentServiceMock.Setup(x => x.GetById(123)).Returns(() => null);

                var publishedSnapshot = Mock.Of<IPublishedSnapshotService>();
                var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<DataEditor>()));
                var usersController = new ContentController(publishedSnapshot, propertyEditorCollection);
                return usersController;
            }

            var runner = new TestRunner(Factory);
            var response = await runner.Execute("Content", "PostSave", HttpMethod.Post,
                content: GetMultiPartRequestContent(PublishJson1),
                mediaTypeHeader: new MediaTypeWithQualityHeaderValue("multipart/form-data"),
                assertOkResponse: false);

            Assert.AreEqual(HttpStatusCode.NotFound, response.Item1.StatusCode);
            Assert.AreEqual(")]}',\n{\"Message\":\"content was not found\"}", response.Item1.Content.ReadAsStringAsync().Result);

            //var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            //Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async Task PostSave_Validate_At_Least_One_Variant_Flagged_For_Saving()
        {
            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                var contentServiceMock = Mock.Get(Current.Services.ContentService);
                contentServiceMock.Setup(x => x.GetById(123)).Returns(() => null);

                var publishedSnapshot = Mock.Of<IPublishedSnapshotService>();
                var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<DataEditor>()));
                var usersController = new ContentController(publishedSnapshot, propertyEditorCollection);
                return usersController;
            }

            var json = JsonConvert.DeserializeObject<JObject>(PublishJson1);
            //remove all save flaggs
            ((JArray)json["variants"])[2]["save"] = false;

            var runner = new TestRunner(Factory);
            var response = await runner.Execute("Content", "PostSave", HttpMethod.Post,
                content: GetMultiPartRequestContent(JsonConvert.SerializeObject(json)),
                mediaTypeHeader: new MediaTypeWithQualityHeaderValue("multipart/form-data"),
                assertOkResponse: false);

            Assert.AreEqual(HttpStatusCode.NotFound, response.Item1.StatusCode);
            Assert.AreEqual(")]}',\n{\"Message\":\"No variants flagged for saving\"}", response.Item1.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Returns 404 if any of the posted properties dont actually exist
        /// </summary>
        /// <returns></returns>
        [Test, Ignore("Not implemented yet")]
        public async Task PostSave_Validate_Properties_Exist()
        {
            //TODO: Make this work! to finish it, we need to include a property in the POST data that doesn't exist on the content type
            // or change the content type below to not include one of the posted ones

            ApiController Factory(HttpRequestMessage message, UmbracoHelper helper)
            {
                var content = MockedContent.CreateSimpleContent(MockedContentTypes.CreateSimpleContentType());
                content.Id = 123;
                content.Path = "-1,123";

                var contentServiceMock = Mock.Get(Current.Services.ContentService);
                contentServiceMock.Setup(x => x.GetById(123)).Returns(() => null);

                var publishedSnapshot = Mock.Of<IPublishedSnapshotService>();
                var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<DataEditor>()));
                var usersController = new ContentController(publishedSnapshot, propertyEditorCollection);
                return usersController;
            }

            var runner = new TestRunner(Factory);
            var response = await runner.Execute("Content", "PostSave", HttpMethod.Post,
                content: GetMultiPartRequestContent(PublishJson1),
                mediaTypeHeader: new MediaTypeWithQualityHeaderValue("multipart/form-data"),
                assertOkResponse: false);

            Assert.AreEqual(HttpStatusCode.NotFound, response.Item1.StatusCode);

            //var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            //Assert.AreEqual(0, obj.TotalItems);
        }
    }
}
