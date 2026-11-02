import { extractJsonQueryProps } from './extract-json-query-properties.function.js';

/**
 * Walks the JSON queries of a JSON-Path string, in order, and returns the properties of the first one
 * matching the given `find` function.
 * @param {string} path The JSON-Path string to search.
 * @param {(props: Record<string, string>) => boolean} find A function to test each set of extracted properties.
 * @returns {Record<string, string> | undefined} The properties of the first JSON query that matches the find function, or an empty object if none match.
 * @example
 * ```ts
 * const path = `$.values[?(@.key == 'y')].value.contentData[?(@.alias == 'x')]`;
 * const props = extractFirstJsonQueryContaining(path, (props) => props.alias !== undefined);
 * console.log(props); // { alias: 'x' }
 * ```
 */
export function extractFirstJsonQueryContaining(
	path: string,
	find: (props: Record<string, string>) => boolean,
): Record<string, string> | undefined {
	const start = path.indexOf('[');
	if (start === -1) return undefined;

	const end = path.indexOf(']', start + 1);
	if (end === -1) return undefined;

	const query = path.substring(start + 1, end);
	const props = extractJsonQueryProps(query);

	if (find(props)) return props;

	return extractFirstJsonQueryContaining(path.slice(end + 1), find);
}
