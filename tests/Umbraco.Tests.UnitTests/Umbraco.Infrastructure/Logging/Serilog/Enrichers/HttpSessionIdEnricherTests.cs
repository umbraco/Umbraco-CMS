// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Logging.Serilog.Enrichers;

[TestFixture]
public class HttpSessionIdEnricherTests
{
    [Test]
    public void Cannot_Recurse_When_Resolving_Session_Id_Logs()
    {
        // Reproduces #16744: resolving the session id raises a nested log event (as a failing
        // DistributedSession.Load() does), which is enriched by this same enricher. Without the
        // re-entrancy guard the enricher resolves the id again and recurses until the stack overflows.
        var resolver = new ReentrantSessionIdResolver();
        var enricher = new HttpSessionIdEnricher(resolver);
        resolver.Enricher = enricher;

        enricher.Enrich(CreateLogEvent(), new PassthroughPropertyFactory());

        Assert.AreEqual(
            1,
            resolver.SessionIdReadCount,
            "The session id must be resolved once; a nested log event during resolution must not re-resolve it.");
    }

    [Test]
    public void Can_Add_Session_Id_Property()
    {
        var enricher = new HttpSessionIdEnricher(new FixedSessionIdResolver("the-session-id"));
        LogEvent logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new PassthroughPropertyFactory());

        Assert.IsTrue(logEvent.Properties.TryGetValue(HttpSessionIdEnricher.HttpSessionIdPropertyName, out LogEventPropertyValue? value));
        Assert.AreEqual("the-session-id", ((ScalarValue)value!).Value);
    }

    [Test]
    public void Cannot_Add_Property_When_Session_Id_Is_Null()
    {
        var enricher = new HttpSessionIdEnricher(new FixedSessionIdResolver(null));
        LogEvent logEvent = CreateLogEvent();

        enricher.Enrich(logEvent, new PassthroughPropertyFactory());

        Assert.IsFalse(logEvent.Properties.ContainsKey(HttpSessionIdEnricher.HttpSessionIdPropertyName));
    }

    private static LogEvent CreateLogEvent() =>
        new(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            new MessageTemplateParser().Parse("test"),
            Array.Empty<LogEventProperty>());

    private sealed class FixedSessionIdResolver(string? sessionId) : ISessionIdResolver
    {
        public string? SessionId { get; } = sessionId;
    }

    private sealed class ReentrantSessionIdResolver : ISessionIdResolver
    {
        private int _depth;

        public HttpSessionIdEnricher? Enricher { get; set; }

        public int SessionIdReadCount { get; private set; }

        public string? SessionId
        {
            get
            {
                SessionIdReadCount++;

                // Simulate the log-during-load that triggers the recursion, but cap the depth so the
                // unguarded code path terminates cleanly for the test rather than overflowing the stack.
                // 50 is well below the frame depth a StackOverflowException needs (~hundreds to low
                // thousands), so it reliably distinguishes "guarded" (one read) from "unguarded" (many
                // reads) without ever actually overflowing the test host.
                if (_depth++ < 50)
                {
                    Enricher!.Enrich(CreateLogEvent(), new PassthroughPropertyFactory());
                }

                return "the-session-id";
            }
        }
    }

    private sealed class PassthroughPropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
            => new(name, new ScalarValue(value));
    }
}
