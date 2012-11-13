using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace Umbraco.Core
{
	/// <summary>
	/// A class that is used to parse routes in the route table to determine which route paths to ignore in the GlobalSettings.
	/// </summary>
	/// <remarks>
	/// The parser is not that intelligent, it will not work for very complex URL structures. It will only support simple URL structures that
	/// have consecutive tokens. 
	/// </remarks>
	/// <example>
	/// Example routes that will parse:
	/// 
	/// /Test/{controller}/{action}/{id}
	/// /Test/MyController/{action}/{id}
	/// 
	/// </example>
	internal class RouteParser
	{
		private readonly RouteCollection _routes;
		private readonly Regex _matchAction = new Regex(@"\{action\}", RegexOptions.Compiled);
		private readonly Regex _matchController = new Regex(@"\{controller\}", RegexOptions.Compiled);
		private readonly Regex _matchId = new Regex(@"\{id\}", RegexOptions.Compiled);

		public RouteParser(RouteCollection routes)
		{
			_routes = routes;
		}

		/// <summary>
		/// Returned all successfully parsed virtual urls from the route collection
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> ParsedVirtualUrlsFromRouteTable()
		{
			return _routes.OfType<Route>()
				.Select(TryGetBaseVirtualPath)
				.Where(x => x.Success)
				.Select(x => x.Result);
		} 

		internal Attempt<string> TryGetBaseVirtualPath(Route route)
		{
			var url = route.Url;
			var controllers = _matchController.Matches(url);
			var actions = _matchAction.Matches(url);
			var ids = _matchId.Matches(url);
			if (controllers.Count > 1)
				return new Attempt<string>(new InvalidOperationException("The URL cannot be parsed, it must contain zero or one {controller} tokens"));
			if (actions.Count != 1)
				return new Attempt<string>(new InvalidOperationException("The URL cannot be parsed, it must contain one {action} tokens"));
			if (ids.Count != 1)
				return new Attempt<string>(new InvalidOperationException("The URL cannot be parsed, it must contain one {id} tokens"));

			//now we need to validate that the order
			if (controllers.Count == 1)
			{
				//actions must occur after controllers
				if (actions[0].Index < controllers[0].Index)
					return new Attempt<string>(new InvalidOperationException("The {action} token must be placed after the {controller} token"));
			}
			//ids must occur after actions
			if (ids[0].Index < actions[0].Index)
				return new Attempt<string>(new InvalidOperationException("The {id} token must be placed after the {action} token"));
			
			//this is all validated, so now we need to return the 'base' virtual path of the route
			return new Attempt<string>(
				true,
				"~/" + url.Substring(0, (controllers.Count == 1 ? controllers[0].Index : actions[0].Index))
					       .TrimEnd('/'));
		}
	}
}
