/* eslint-disable */
import { stripSlash } from '@umbraco-cms/backoffice/external/router-slot'; // This must only include the util to avoid side effects of registering the route element.

const PARAM_IDENTIFIER = /:([^\\/]+)/g;

export function generateRoutePathBuilder(path: string) {
	return (params: { [key: string]: string | number } | null) => {
		return '/' + stripSlash(
				params ?
					path.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
						return params[args[0]].toString();
					}
			  ): path) + '/';
	}
}
