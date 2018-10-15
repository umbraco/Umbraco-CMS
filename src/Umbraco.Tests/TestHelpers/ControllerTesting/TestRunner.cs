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
        private readonly Func<HttpRequestMessage, UmbracoHelper, ApiController> _controllerFactory;

        public TestRunner(Func<HttpRequestMessage, UmbracoHelper, ApiController> controllerFactory)
        {
            _controllerFactory = controllerFactory;
        }

        public async Task<Tuple<HttpResponseMessage, string>> Execute(string controllerName, string actionName, HttpMethod method, HttpContent content = null)
        {
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

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine(request);
                var response = await server.HttpClient.SendAsync(request);
                Console.WriteLine(response);

                string json = "";
                if (response.IsSuccessStatusCode == false)
                {
                    WriteResponseError(response);
                }
                else
                {
                    json = (await ((StreamContent)response.Content).ReadAsStringAsync()).TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
                    var deserialized = JsonConvert.DeserializeObject(json);
                    Console.Write(JsonConvert.SerializeObject(deserialized, Formatting.Indented));
                }

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                return Tuple.Create(response, json);
            }
        }

        private static void WriteResponseError(HttpResponseMessage response)
        {
            var result = response.Content.ReadAsStringAsync().Result;
            Console.Out.WriteLine("Http operation unsuccessfull");
            Console.Out.WriteLine(string.Format("Status: '{0}'", response.StatusCode));
            Console.Out.WriteLine(string.Format("Reason: '{0}'", response.ReasonPhrase));
            Console.Out.WriteLine(result);
        }
    }
}