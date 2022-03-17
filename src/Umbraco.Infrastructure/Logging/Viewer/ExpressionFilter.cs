using System;
using System.Linq;
using Serilog.Events;
using Serilog.Expressions;
using Umbraco.Cms.Infrastructure.Logging.Viewer;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Viewer
{
    //Log Expression Filters (pass in filter exp string)
    internal class ExpressionFilter : ILogFilter
    {
        private readonly Func<LogEvent, bool>? _filter;
        private const string s_expressionOperators = "()+=*<>%-";

        public ExpressionFilter(string? filterExpression)
        {
            Func<LogEvent, bool>? filter;

            // Our custom Serilog Functions to extend Serilog.Expressions
            // In this case we are plugging the gap for the missing Has()
            // function from porting away from Serilog.Filters.Expressions to Serilog.Expressions
            // Along with patching support for the more verbose built in property names
            var customSerilogFunctions = new SerilogLegacyNameResolver(typeof(SerilogExpressionsFunctions));

            if (string.IsNullOrEmpty(filterExpression))
            {
                return;
            }

            // If the expression is one word and doesn't contain a serilog operator then we can perform a like search
            if (!filterExpression.Contains(" ") && !filterExpression.ContainsAny(s_expressionOperators.Select(c => c)))
            {
                filter = PerformMessageLikeFilter(filterExpression);
            }
            else // check if it's a valid expression
            {
                // If the expression evaluates then make it into a filter
                if (SerilogExpression.TryCompile(filterExpression, null, customSerilogFunctions, out CompiledExpression? compiled, out var error))
                {
                    filter = evt =>
                    {
                        LogEventPropertyValue? result = compiled(evt);
                        return ExpressionResult.IsTrue(result);
                    };
                }
                else
                {
                    // 'error' describes a syntax error, where it was unable to compile an expression
                    // Assume the expression was a search string and make a Like filter from that
                    filter = PerformMessageLikeFilter(filterExpression);
                }
            }

            _filter = filter;
        }

        public bool TakeLogEvent(LogEvent e)
        {
            return _filter == null || _filter(e);
        }

        private Func<LogEvent, bool>? PerformMessageLikeFilter(string filterExpression)
        {
            var filterSearch = $"@Message like '%{SerilogExpression.EscapeLikeExpressionContent(filterExpression)}%'";
            if (SerilogExpression.TryCompile(filterSearch, out CompiledExpression? compiled, out var error))
            {
                // `compiled` is a function that can be executed against `LogEvent`s:
                return evt =>
                {
                    LogEventPropertyValue? result = compiled(evt);
                    return ExpressionResult.IsTrue(result);
                };
            }

            return null;
        }
    }
}
