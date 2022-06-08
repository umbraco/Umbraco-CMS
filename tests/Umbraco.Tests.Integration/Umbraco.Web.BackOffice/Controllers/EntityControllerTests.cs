using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class EntityControllerTests : UmbracoTestServerTestBase
    {
        private IScopeProvider ScopeProvider => GetRequiredService<IScopeProvider>();

        [Test]
        public async Task GetUrlsByIds_MediaWithIntegerIds_ReturnsValidMap()
        {
            IMediaTypeService mediaTypeService = Services.GetRequiredService<IMediaTypeService>();
            IMediaService mediaService = Services.GetRequiredService<IMediaService>();

            var mediaItems = new List<Media>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IMediaType mediaType = mediaTypeService.Get("image");
                mediaTypeService.Save(mediaType);

                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType,  -1));
                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType, -1));

                foreach (Media media in mediaItems)
                {
                    mediaService.Save(media);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Media
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    mediaItems[0].Id,
                    mediaItems[1].Id,
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<int, string> results = await response.Content.ReadFromJsonAsync<IDictionary<int, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/media"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/media"));
            });
        }

        [Test]
        public async Task GetUrlsByIds_Media_ReturnsEmptyStringsInMapForUnknownItems()
        {
            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Media
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[] { 1, 2 }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<int, string> results = await response.Content.ReadFromJsonAsync<IDictionary<int, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.That(results!.Keys.Count, Is.EqualTo(2));
                Assert.AreEqual(results![payload.ids[0]], string.Empty);
            });
        }

        [Test]
        public async Task GetUrlsByIds_MediaWithGuidIds_ReturnsValidMap()
        {
            IMediaTypeService mediaTypeService = Services.GetRequiredService<IMediaTypeService>();
            IMediaService mediaService = Services.GetRequiredService<IMediaService>();

            var mediaItems = new List<Media>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IMediaType mediaType = mediaTypeService.Get("image");
                mediaTypeService.Save(mediaType);

                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType, -1));
                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType, -1));

                foreach (Media media in mediaItems)
                {
                    mediaService.Save(media);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Media
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    mediaItems[0].Key.ToString(),
                    mediaItems[1].Key.ToString(),
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<string, string> results = await response.Content.ReadFromJsonAsync<IDictionary<string, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/media"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/media"));
            });
        }

        [Test]
        public async Task GetUrlsByIds_MediaWithUdiIds_ReturnsValidMap()
        {
            IMediaTypeService mediaTypeService = Services.GetRequiredService<IMediaTypeService>();
            IMediaService mediaService = Services.GetRequiredService<IMediaService>();

            var mediaItems = new List<Media>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IMediaType mediaType = mediaTypeService.Get("image");
                mediaTypeService.Save(mediaType);

                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType, -1));
                mediaItems.Add(MediaBuilder.CreateMediaImage(mediaType, -1));

                foreach (Media media in mediaItems)
                {
                    mediaService.Save(media);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Media
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    mediaItems[0].GetUdi().ToString(),
                    mediaItems[1].GetUdi().ToString(),
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<string, string> results = await response.Content.ReadFromJsonAsync<IDictionary<string, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/media"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/media"));
            });
        }

        [Test]
        public async Task GetUrlsByIds_Documents_ReturnsHashesInMapForUnknownItems()
        {
            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Document
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[] { 1, 2 }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<int, string> results = await response.Content.ReadFromJsonAsync<IDictionary<int, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.That(results!.Keys.Count, Is.EqualTo(2));
                Assert.AreEqual(results![payload.ids[0]], "#");
            });
        }

        [Test]
        public async Task GetUrlsByIds_DocumentWithIntIds_ReturnsValidMap()
        {
            IContentTypeService contentTypeService = Services.GetRequiredService<IContentTypeService>();
            IContentService contentService = Services.GetRequiredService<IContentService>();

            var contentItems = new List<IContent>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
                contentTypeService.Save(contentType);

                ContentBuilder builder = new ContentBuilder()
                    .WithContentType(contentType);

                Content root = builder.WithName("foo").Build();
                contentService.SaveAndPublish(root);

                contentItems.Add(builder.WithParent(root).WithName("bar").Build());
                contentItems.Add(builder.WithParent(root).WithName("baz").Build());

                foreach (IContent content in contentItems)
                {
                    contentService.SaveAndPublish(content);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Document
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    contentItems[0].Id,
                    contentItems[1].Id,
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<int, string> results = await response.Content.ReadFromJsonAsync<IDictionary<int, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/bar"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/baz"));
            });
        }

        [Test]
        public async Task GetUrlsByIds_DocumentWithGuidIds_ReturnsValidMap()
        {
            IContentTypeService contentTypeService = Services.GetRequiredService<IContentTypeService>();
            IContentService contentService = Services.GetRequiredService<IContentService>();

            var contentItems = new List<IContent>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
                contentTypeService.Save(contentType);

                ContentBuilder builder = new ContentBuilder()
                    .WithContentType(contentType);

                Content root = builder.WithName("foo").Build();
                contentService.SaveAndPublish(root);

                contentItems.Add(builder.WithParent(root).WithName("bar").Build());
                contentItems.Add(builder.WithParent(root).WithName("baz").Build());

                foreach (IContent content in contentItems)
                {
                    contentService.SaveAndPublish(content);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Document
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    contentItems[0].Key.ToString(),
                    contentItems[1].Key.ToString(),
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<string, string> results = await response.Content.ReadFromJsonAsync<IDictionary<string, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/bar"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/baz"));
            });
        }

        [Test]
        public async Task GetUrlsByIds_DocumentWithUdiIds_ReturnsValidMap()
        {
            IContentTypeService contentTypeService = Services.GetRequiredService<IContentTypeService>();
            IContentService contentService = Services.GetRequiredService<IContentService>();

            var contentItems = new List<IContent>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
                contentTypeService.Save(contentType);

                ContentBuilder builder = new ContentBuilder()
                    .WithContentType(contentType);

                Content root = builder.WithName("foo").Build();
                contentService.SaveAndPublish(root);

                contentItems.Add(builder.WithParent(root).WithName("bar").Build());
                contentItems.Add(builder.WithParent(root).WithName("baz").Build());

                foreach (IContent content in contentItems)
                {
                    contentService.SaveAndPublish(content);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Document
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetUrlsByIds", typeof(EntityController), queryParameters);

            var payload = new
            {
                ids = new[]
                {
                    contentItems[0].GetUdi().ToString(),
                    contentItems[1].GetUdi().ToString(),
                }
            };

            HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, payload);

            // skip pointless un-parseable cruft.
            (await response.Content.ReadAsStreamAsync()).Seek(AngularJsonMediaTypeFormatter.XsrfPrefix.Length, SeekOrigin.Begin);

            IDictionary<string, string> results = await response.Content.ReadFromJsonAsync<IDictionary<string, string>>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(results![payload.ids[0]].StartsWith("/bar"));
                Assert.IsTrue(results![payload.ids[1]].StartsWith("/baz"));
            });
        }

        [Test]
        public async Task GetByIds_MultipleCalls_WorksAsExpected()
        {
            IContentTypeService contentTypeService = Services.GetRequiredService<IContentTypeService>();
            IContentService contentService = Services.GetRequiredService<IContentService>();

            var contentItems = new List<IContent>();

            using (IScope scope = ScopeProvider.CreateScope())
            {
                IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
                contentTypeService.Save(contentType);

                ContentBuilder builder = new ContentBuilder()
                    .WithContentType(contentType);

                Content root = builder.WithName("foo").Build();
                contentService.SaveAndPublish(root);

                contentItems.Add(builder.WithParent(root).WithName("bar").Build());
                contentItems.Add(builder.WithParent(root).WithName("baz").Build());

                foreach (IContent content in contentItems)
                {
                    contentService.SaveAndPublish(content);
                }

                scope.Complete();
            }

            var queryParameters = new Dictionary<string, object>
            {
                ["type"] = Constants.UdiEntityType.Document
            };

            var url = LinkGenerator.GetUmbracoControllerUrl("GetByIds", typeof(EntityController), queryParameters);

            var udiPayload = new
            {
                ids = new[]
                {
                    contentItems[0].GetUdi().ToString(),
                    contentItems[1].GetUdi().ToString(),
                }
            };

            var intPayload = new
            {
                ids = new[]
                {
                    contentItems[0].Id,
                    contentItems[1].Id,
                }
            };

            HttpResponseMessage udiResponse = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, udiPayload);
            HttpResponseMessage intResponse = await HttpClientJsonExtensions.PostAsJsonAsync(Client, url, intPayload);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, udiResponse.StatusCode, "First request error");
                Assert.AreEqual(HttpStatusCode.OK, intResponse.StatusCode, "Second request error");
            });
        }
    }
}
