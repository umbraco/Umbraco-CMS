// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.AspNetCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AspNetCore;

[TestFixture]
public class AspNetCoreSessionManagerTests
{
    private const string SessionCookieName = "UMB_SESSION";
    private const string SessionId = "test-session-id";

    [Test]
    public void SessionId_Returns_Zero_When_Sessions_Not_Available()
    {
        var httpContext = new DefaultHttpContext();
        var sessionManager = CreateSessionManager(httpContext);

        Assert.AreEqual("0", sessionManager.SessionId);
    }

    [Test]
    public void SessionId_Does_Not_Load_Session_When_No_Session_Cookie_Present()
    {
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        var sessionManager = CreateSessionManager(httpContext);

        var result = sessionManager.SessionId;

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.AreEqual(0, session.IdReadCount, "Session.Id must not be read when no session cookie is present, as that forces a blocking store load.");
        });
    }

    [Test]
    public void SessionId_Returns_Id_When_Session_Cookie_Present()
    {
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        httpContext.Request.Headers.Append(HeaderNames.Cookie, $"{SessionCookieName}={SessionId}");
        var sessionManager = CreateSessionManager(httpContext);

        var result = sessionManager.SessionId;

        Assert.AreEqual(SessionId, result);
    }

    private static AspNetCoreSessionManager CreateSessionManager(DefaultHttpContext httpContext)
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        IOptions<SessionOptions> sessionOptions = Options.Create(new SessionOptions { Cookie = { Name = SessionCookieName } });
        return new AspNetCoreSessionManager(httpContextAccessor, sessionOptions);
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public TestSessionFeature(ISession session) => Session = session;

        public ISession Session { get; set; }
    }

    private sealed class TrackingSession(string id) : ISession
    {
        public int IdReadCount { get; private set; }

        public string Id
        {
            get
            {
                IdReadCount++;
                return id;
            }
        }

        public bool IsAvailable => true;

        public IEnumerable<string> Keys => Array.Empty<string>();

        public void Clear()
        {
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key)
        {
        }

        public void Set(string key, byte[] value)
        {
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            value = Array.Empty<byte>();
            return false;
        }
    }
}
