const propValueRegex = /@\.([a-zA-Z_$][\w$]*)\s*==\s*['"]([^'"]*)['"]/g;

/**
 * Extracts properties and their values from a JSON path query.
 * @param {string} query - The JSON path query.
 * @returns {Record<string, string>} An object containing the properties and their values.
 * @example
 * ```ts
 * const query = `?(@.culture == 'en-us' && @.segment == 'mySegment')`;
 * const props = ExtractJsonQueryProps(query);
 * console.log(props); // { culture: 'en-us', segment: 'mySegment' }
 * ```
 */
export function ExtractJsonQueryProps(query: string): Record<string, string> {
	// Object to hold property-value pairs
	const propsMap: Record<string, string> = {};
	let match;

	// Iterate over all matches
	while ((match = propValueRegex.exec(query)) !== null) {
		propsMap[match[1]] = match[2];
	}

	return propsMap;
}
