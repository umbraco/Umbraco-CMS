import type { ISlashOptions } from '@umbraco-cms/backoffice/router';

const PARAM_IDENTIFIER = /:([^\\/]+)/g;

function stripSlash(path: string): string {
	return slashify(path, { start: false, end: false });
}

function slashify(path: string, { start = true, end = true }: Partial<ISlashOptions> = {}): string {
	path = start && !path.startsWith('/') ? `/${path}` : !start && path.startsWith('/') ? path.slice(1) : path;
	return end && !path.endsWith('/') ? `${path}/` : !end && path.endsWith('/') ? path.slice(0, path.length - 1) : path;
}

export function generateRoutePathBuilder(path: string) {
	return (params: { [key: string]: string | number }) =>
		stripSlash(
			path.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
				return params[args[0]].toString();
			})
		);
}
