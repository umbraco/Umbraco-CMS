import type { Path } from 'msw';
export function umbracoPath(path: string): Path {
	return `/umbraco/management/api/v1${path}`;
}
