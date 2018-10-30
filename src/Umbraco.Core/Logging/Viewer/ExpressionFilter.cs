using System;
using System.Linq;
using Serilog.Events;
using Serilog.Filters.Expressions;

namespace Umbraco.Core.Logging.Viewer
{
    //Log Expression Filters (pass in filter exp string)
    public class ExpressionFilter : ILogFilter
    {
        private Func<LogEvent, bool> _filter;
        private readonly string expressionOperators = "()+=*<>%-";

        public ExpressionFilter(string filterExpression)
        {
            Func<LogEvent, bool> filter = null;

            if (string.IsNullOrEmpty(filterExpression) == false)
            {
                // If the expression is one word and doesn't contain a serilog operator then we can perform a like search
                if (!filterExpression.Contains(" ") && !filterExpression.ContainsAny(expressionOperators.Select(c => c)))
                {
                    filter = PerformMessageLikeFilter(filterExpression);
                }
                else // check if it's a valid expression
                {
                    // If the expression evaluates then make it into a filter
                    if (FilterLanguage.TryCreateFilter(filterExpression, out Func<LogEvent, object> eval, out string error))
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
        }
    
        public bool TakeLogEvent(LogEvent e)
        {
            if(_filter == null)
            {
                //If no filter has been setup - take all log items
                return true;
            }

            return _filter(e);
        }

        private Func<LogEvent, bool> PerformMessageLikeFilter(string filterExpression)
        {
            var filterSearch = $"@Message like '%{FilterLanguage.EscapeLikeExpressionContent(filterExpression)}%'";
            if (FilterLanguage.TryCreateFilter(filterSearch, out var eval, out var error))
            {
                return evt => true.Equals(eval(evt));
            }

            return null;
        }
    }
}
