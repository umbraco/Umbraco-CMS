using System;
using System.Linq;
using Serilog.Events;
using Serilog.Filters.Expressions;

namespace Umbraco.Core.Logging.Viewer
{
    //Log Expression Filters (pass in filter exp string)
    internal class ExpressionFilter : ILogFilter
    {
        private readonly Func<LogEvent, bool> _filter;
        private const string ExpressionOperators = "()+=*<>%-";

        public ExpressionFilter(string filterExpression)
        {
            Func<LogEvent, bool> filter;

            if (string.IsNullOrEmpty(filterExpression))
            {
                return;
            }

            // If the expression is one word and doesn't contain a serilog operator then we can perform a like search
            if (!filterExpression.Contains(" ") && !filterExpression.ContainsAny(ExpressionOperators.Select(c => c)))
            {
                filter = PerformMessageLikeFilter(filterExpression);
            }
            else // check if it's a valid expression
            {
                // If the expression evaluates then make it into a filter
                if (FilterLanguage.TryCreateFilter(filterExpression, out var eval, out _))
                {
                    filter = evt => true.Equals(eval(evt));
                }
                else
                {
                    //Assume the expression was a search string and make a Like filter from that
                    filter = PerformMessageLikeFilter(filterExpression);
                }
            }

            _filter = filter;
        }

        public bool TakeLogEvent(LogEvent e)
        {
            return _filter == null || _filter(e);
        }

        private Func<LogEvent, bool> PerformMessageLikeFilter(string filterExpression)
        {
            var filterSearch = $"@Message like '%{FilterLanguage.EscapeLikeExpressionContent(filterExpression)}%'";
            if (FilterLanguage.TryCreateFilter(filterSearch, out var eval, out _))
            {
                return evt => true.Equals(eval(evt));
            }

            return null;
        }
    }
}
