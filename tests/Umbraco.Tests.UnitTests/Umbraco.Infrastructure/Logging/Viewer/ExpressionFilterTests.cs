// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Serilog.Events;
using Serilog.Parsing;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Logging.Viewer;

[TestFixture]
public class ExpressionFilterTests
{
    [Test]
    public void Can_Match_Log_Event_With_Single_Word_Filter()
    {
        var filter = new ExpressionFilter("hello");

        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("say hello world")));
        Assert.IsFalse(filter.TakeLogEvent(CreateLogEvent("say goodbye world")));
    }

    [Test]
    public void Can_Match_Log_Event_With_Expression_Filter()
    {
        // Contains an operator, so it is compiled as an expression rather than searched for as a term.
        var filter = new ExpressionFilter("@Level='Error'");

        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("boom", LogEventLevel.Error)));
        Assert.IsFalse(filter.TakeLogEvent(CreateLogEvent("boom", LogEventLevel.Information)));
    }

    [Test]
    public void Can_Match_Log_Event_With_Expression_Filter_Containing_Spaces()
    {
        var filter = new ExpressionFilter("@Level = 'Error'");

        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("boom", LogEventLevel.Error)));
        Assert.IsFalse(filter.TakeLogEvent(CreateLogEvent("boom", LogEventLevel.Information)));
    }

    [TestCase("")]
    [TestCase(null)]
    public void Empty_Filter_Takes_All_Log_Events(string? filterExpression)
    {
        var filter = new ExpressionFilter(filterExpression);

        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("anything at all")));
        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("boom", LogEventLevel.Error)));
    }

    [Test]
    public void Can_Match_Log_Event_With_Filter_Containing_Term_Only_Characters()
    {
        // None of these are operators, so the whole thing stays a term to search the message for.
        var filter = new ExpressionFilter("Umbraco.Cms_Core:42");

        Assert.IsTrue(filter.TakeLogEvent(CreateLogEvent("from Umbraco.Cms_Core:42 today")));
        Assert.IsFalse(filter.TakeLogEvent(CreateLogEvent("from Umbraco.Cms_Core:43 today")));
    }

    private static LogEvent CreateLogEvent(string message, LogEventLevel level = LogEventLevel.Information)
        => new(
            DateTimeOffset.UtcNow,
            level,
            exception: null,
            new MessageTemplateParser().Parse(message),
            []);
}
