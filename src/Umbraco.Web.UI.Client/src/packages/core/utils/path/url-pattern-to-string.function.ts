export type UrlParametersRecord = Record<string, string | number | { toString: () => string }>;

const PARAM_IDENTIFIER = /:([^/]+)/g;

export function umbUrlPatternToString(pattern: string, params: UrlParametersRecord | null): string {
	return params
		? pattern.replace(PARAM_IDENTIFIER, (_substring: string, ...args: string[]) => {
				// Replace the parameter with the value from the params object or the parameter name if it doesn't exist (args[0] is the parameter name without the colon)
				return typeof params[args[0]] !== 'undefined' ? params[args[0]].toString() : `:${args[0]}`;
			})
		: pattern;
}
