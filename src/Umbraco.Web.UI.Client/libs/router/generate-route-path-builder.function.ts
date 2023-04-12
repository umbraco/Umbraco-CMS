import { stripSlash } from '@umbraco-cms/backoffice/router';

const PARAM_IDENTIFIER = /:([^\\/]+)/g;

export function generateRoutePathBuilder(path: string) {
	return (params: { [key: string]: string | number }) =>
		stripSlash(
			path.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
				return params[args[0]].toString();
			})
		);
}
