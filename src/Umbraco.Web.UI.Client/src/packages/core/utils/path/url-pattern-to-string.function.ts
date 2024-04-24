export type UrlParametersRecord = Record<string, string | number | { toString: () => string } | null>;

const PARAM_IDENTIFIER = /:([^/]+)/g;

export function umbUrlPatternToString(pattern: string, params: UrlParametersRecord | null): string {
	return params
		? pattern.replace(PARAM_IDENTIFIER, (_substring: string, ...args: string[]) => {
				const segmentName = args[0]; // (segmentName is the parameter name without the colon)
				// Replace the path-segment with the value from the params object or 'null' if it doesn't exist
				return params![segmentName]?.toString() ?? 'null';
			})
		: pattern;
}
