import { stripSlash } from './router-slot/util.js';
import { umbUrlPatternToString, type UrlParametersRecord } from '@umbraco-cms/backoffice/utils';

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
