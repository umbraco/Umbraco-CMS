import { type UrlParametersRecord, umbUrlPatternToString } from '../utils/path/url-pattern-to-string.function.js';
import { stripSlash } from '@umbraco-cms/backoffice/external/router-slot'; // This must only include the util to avoid side effects of registering the route element.

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
