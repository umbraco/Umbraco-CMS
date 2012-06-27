using System;
using System.Web;
using TypeMock.ArrangeActAssert;
using System.IO;

namespace umbraco.Linq.Core.Tests
{
    internal class MockHelpers
    {
        public static void SetupFakeHttpContext()
        {
            HttpContext fakeHttpContext = Isolate.Fake.Instance<HttpContext>(Members.MustSpecifyReturnValues);
            HttpServerUtility fakeServer = Isolate.Fake.Instance<HttpServerUtility>();
            Isolate.WhenCalled(() => HttpContext.Current).WillReturn(fakeHttpContext);
            Isolate.WhenCalled(() => fakeHttpContext.Server).WillReturn(fakeServer);
            Isolate.WhenCalled(() => fakeServer.MapPath(string.Empty)).WillReturn(Path.Combine(Environment.CurrentDirectory, "umbraco.config"));
        }
    }
}
