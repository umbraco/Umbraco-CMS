using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    public class TestRunner
    {
        private readonly Func<HttpRequestMessage, UmbracoContext, UmbracoHelper, ApiController> _controllerFactory;

        public TestRunner(Func<HttpRequestMessage, UmbracoContext, UmbracoHelper, ApiController> controllerFactory)
        {
            _controllerFactory = controllerFactory;
        }

        public async Task<Tuple<HttpResponseMessage, string>> Execute(string controllerName, string actionName, HttpMethod method,
            HttpContent content = null,
            MediaTypeWithQualityHeaderValue mediaTypeHeader = null,
            bool assertOkResponse = true)
        {
            if (mediaTypeHeader == null)
            {
                mediaTypeHeader = new MediaTypeWithQualityHeaderValue("application/json");
            }

            var startup = new TestStartup(
                configuration =>
                {
                    configuration.Routes.MapHttpRoute("Default",
                        routeTemplate: "{controller}/{action}/{id}",
                        defaults: new { controller = controllerName, action = actionName, id = RouteParameter.Optional });
                },
                _controllerFactory);

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://testserver/"),
                    Method = method
                };

                if (content != null)
                    request.Content = content;

                request.Headers.Accept.Add(mediaTypeHeader);

                Console.WriteLine(request);
                var response = await server.HttpClient.SendAsync(request);
                Console.WriteLine(response);

                if (response.IsSuccessStatusCode == false)
                {
                    WriteResponseError(response);
                }

                var json = (await ((StreamContent)response.Content).ReadAsStringAsync()).TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
                if (!json.IsNullOrWhiteSpace())
                {
                    var deserialized = JsonConvert.DeserializeObject(json);
                    Console.Write(JsonConvert.SerializeObject(deserialized, Formatting.Indented));
                }

                if (assertOkResponse)
                {
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                }
                
                return Tuple.Create(response, json);
            }
        }

        private static void WriteResponseError(HttpResponseMessage response)
        {
            var result = response.Content.ReadAsStringAsync().Result;
            Console.Out.WriteLine("Http operation unsuccessfull");
            Console.Out.WriteLine($"Status: '{response.StatusCode}'");
            Console.Out.WriteLine($"Reason: '{response.ReasonPhrase}'");
            Console.Out.WriteLine(result);
        }
    }
}
