// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AspNetCore;

[TestFixture]
public class AspNetCoreSessionManagerTests
{
    private const string SessionCookieName = "UMB_SESSION";
    private const string SessionId = "test-session-id";

    [Test]
    public void Can_Resolve_SessionId_As_Zero_When_Sessions_Not_Available()
    {
        var httpContext = new DefaultHttpContext();
        var sessionManager = CreateSessionManager(httpContext);

        Assert.AreEqual("0", sessionManager.SessionId);
    }

    [Test]
    public void Cannot_Load_Session_Without_Cookie_Present()
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
    public void Can_Resolve_SessionId_When_Session_Cookie_Present()
    {
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        httpContext.Request.Headers.Append(HeaderNames.Cookie, $"{SessionCookieName}={SessionId}");
        var sessionManager = CreateSessionManager(httpContext);

        var result = sessionManager.SessionId;

        Assert.AreEqual(SessionId, result);
    }

    [Test]
    public void Does_Not_Resolve_SessionId_When_Logging_Mode_Is_None()
    {
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        httpContext.Request.Headers.Append(HeaderNames.Cookie, $"{SessionCookieName}={SessionId}");
        var sessionManager = CreateSessionManager(httpContext, SessionIdLoggingMode.None);

        var result = sessionManager.SessionId;

        Assert.Multiple(() =>
        {
            Assert.IsNull(result);
            Assert.AreEqual(0, session.IdReadCount, "Session.Id must not be read when the logging mode is None.");
        });
    }

    [Test]
    public void Can_Resolve_Cookie_Hash_When_Session_Cookie_Present()
    {
        const string cookieValue = "the-session-cookie-value";
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        httpContext.Request.Headers.Append(HeaderNames.Cookie, $"{SessionCookieName}={cookieValue}");
        var sessionManager = CreateSessionManager(httpContext, SessionIdLoggingMode.CookieHash);

        var result = sessionManager.SessionId;
        var resultAgain = sessionManager.SessionId;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(cookieValue.GenerateHash<SHA256>(), result);
            Assert.AreNotEqual(cookieValue, result, "The raw cookie value must not be logged.");
            Assert.AreEqual(result, resultAgain, "The hash must be stable across reads.");
            Assert.AreEqual(0, session.IdReadCount, "Session.Id must not be read in CookieHash mode.");
        });
    }

    [Test]
    public void Does_Not_Resolve_Cookie_Hash_Without_Cookie_Present()
    {
        var session = new TrackingSession(SessionId);
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature(session));
        var sessionManager = CreateSessionManager(httpContext, SessionIdLoggingMode.CookieHash);

        Assert.Multiple(() =>
        {
            Assert.IsNull(sessionManager.SessionId);
            Assert.AreEqual(0, session.IdReadCount);
        });
    }

    private static AspNetCoreSessionManager CreateSessionManager(
        DefaultHttpContext httpContext,
        SessionIdLoggingMode mode = SessionIdLoggingMode.SessionId)
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        IOptions<SessionOptions> sessionOptions = Options.Create(new SessionOptions { Cookie = { Name = SessionCookieName } });
        var loggingSettings = new TestOptionsMonitor<LoggingSettings>(new LoggingSettings { SessionIdLogging = mode });
        return new AspNetCoreSessionManager(httpContextAccessor, sessionOptions, loggingSettings);
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
