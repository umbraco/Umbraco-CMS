import { stripSlash } from './router-slot/util.js';
import { umbUrlPatternToString, type UmbUrlParametersRecord } from '@umbraco-cms/backoffice/utils';

/**
 *
 * @param path
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
