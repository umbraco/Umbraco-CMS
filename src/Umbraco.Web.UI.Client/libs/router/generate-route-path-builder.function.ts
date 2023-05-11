/* eslint-disable */
import { stripSlash } from 'router-slot/util'; // This must only include the util to avoid side effects of registering the route element.

const PARAM_IDENTIFIER = /:([^\\/]+)/g;

export function generateRoutePathBuilder(path: string) {
	return (params: { [key: string]: string | number } | null) =>
		params
			? stripSlash(
					path.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
						return params[args[0]].toString();
					})
			  )
			: path;
}
