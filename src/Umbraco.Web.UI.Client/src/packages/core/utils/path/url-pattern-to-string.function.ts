export type UrlParametersRecord = Record<string, string | number | { toString: () => string } | null>;

const PARAM_IDENTIFIER = /:([^/]+)/g;

/**
 *
 * @param pattern
 * @param params
 */
export function umbUrlPatternToString(pattern: string, params: UrlParametersRecord | null): string {
	return params
		? pattern.replace(PARAM_IDENTIFIER, (_substring: string, ...args: string[]) => {
				const segmentValue = params![args[0]]; // (segmentValue is the value to replace the parameter)
				// Replace the path-segment with the value from the params object or 'null' if it doesn't exist
				return segmentValue === undefined ? `:${args[0]}` : segmentValue === null ? 'null' : segmentValue.toString();
			})
		: pattern;
}
