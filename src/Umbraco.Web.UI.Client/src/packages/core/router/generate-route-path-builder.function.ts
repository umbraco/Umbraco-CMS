/* eslint-disable */
import { stripSlash } from '@umbraco-cms/backoffice/external/router-slot'; // This must only include the util to avoid side effects of registering the route element.

const PARAM_IDENTIFIER = /:([^\/]+)/g;

export function createRoutePathBuilder(path: string) {
	return (params: { [key: string]: string | number | { toString: () => string } } | null) => {
		return (
			'/' +
			stripSlash(
				params
					? path.replace(PARAM_IDENTIFIER, (_substring: string, ...args: string[]) => {
							// Replace the parameter with the value from the params object or the parameter name if it doesn't exist (args[0] is the parameter name without the colon)
							return typeof params[args[0]] !== 'undefined' ? params[args[0]].toString() : `:${args[0]}`;
						})
					: path,
			) +
			'/'
		);
	};
}
