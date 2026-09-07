import { stripSlash } from './router-slot/util.js';
import { umbUrlPatternToString, type UmbUrlParametersRecord } from '@umbraco-cms/backoffice/utils';

/**
 * Creates a function that builds an absolute route path from the given path pattern.
 * @param {string} path - The path pattern to build from.
 * @returns {(params: UmbUrlParametersRecord | null) => string} A function that generates the path from a set of params.
 */
export function umbGenerateRoutePathBuilder(path: string) {
	return (params: UmbUrlParametersRecord | null) => {
		return '/' + stripSlash(umbUrlPatternToString(path, params)) + '/';
	};
}

/**
 * @deprecated Use `umbGenerateRoutePathBuilder` instead.
 */
export { umbGenerateRoutePathBuilder as umbCreateRoutePathBuilder };
