import { umbUrlPatternToString, type UrlParametersRecord } from '@umbraco-cms/backoffice/utils';
import { stripSlash } from './router-slot/util.js';

/**
 *
 * @param path
 */
export function umbGenerateRoutePathBuilder(path: string) {
	return (params: UrlParametersRecord | null) => {
		return '/' + stripSlash(umbUrlPatternToString(path, params)) + '/';
	};
}

/**
 * @deprecated Use `umbGenerateRoutePathBuilder` instead.
 */
export { umbGenerateRoutePathBuilder as umbCreateRoutePathBuilder };
