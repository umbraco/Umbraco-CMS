using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Quickly split filters into different types 
    /// </summary>
    internal class FilterGrouping
    {
        private readonly List<IActionFilter> _actionFilters = new List<IActionFilter>();
        private readonly List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
        private readonly List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();

        public FilterGrouping(IEnumerable<FilterInfo> filters)
        {
            if (filters == null) throw new ArgumentNullException("filters");

            foreach (FilterInfo f in filters)
            {
                var filter = f.Instance;
                Categorize(filter, _actionFilters);
                Categorize(filter, _authorizationFilters);
                Categorize(filter, _exceptionFilters);
            }
        }

        public IEnumerable<IActionFilter> ActionFilters
        {
            get { return _actionFilters; }
        }

        public IEnumerable<IAuthorizationFilter> AuthorizationFilters
        {
            get { return _authorizationFilters; }
        }

        public IEnumerable<IExceptionFilter> ExceptionFilters
        {
            get { return _exceptionFilters; }
        }

        private static void Categorize<T>(IFilter filter, List<T> list) where T : class
        {
            T match = filter as T;
            if (match != null)
            {
                list.Add(match);
            }
        }
    }
}
