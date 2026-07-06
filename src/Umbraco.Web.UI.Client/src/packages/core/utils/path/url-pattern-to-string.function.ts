export type UmbUrlParametersRecord = Record<string, string | number | { toString: () => string } | null>;
/**
 * @deprecated Use `UmbUrlParametersRecord` instead. Will be removed in v.18
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export type UrlParametersRecord = UmbUrlParametersRecord;

const PARAM_IDENTIFIER = /:([^/]+)/g;

/**
 * Resolves the `:parameter` tokens in a URL pattern using the given parameter values.
 * @param {string} pattern - A URL pattern where parameters are written as `:name` (e.g. `/section/:id/edit`).
 * @param {UmbUrlParametersRecord | null} params - Key value object, where keys are parameter names and values are the values to replace them with.
 * @returns {string} The pattern with its parameter tokens replaced.
 * @example
 * umbUrlPatternToString('/section/:id/edit', { id: 123 }); // '/section/123/edit'
 */
export function umbUrlPatternToString(pattern: string, params: UmbUrlParametersRecord | null): string {
	return params
		? pattern.replace(PARAM_IDENTIFIER, (_substring: string, ...args: string[]) => {
				const segmentValue = params![args[0]]; // (segmentValue is the value to replace the parameter)
				// Replace the path-segment with the value from the params object or 'null' if it doesn't exist
				return segmentValue === undefined ? `:${args[0]}` : segmentValue === null ? 'null' : segmentValue.toString();
			})
		: pattern;
}
