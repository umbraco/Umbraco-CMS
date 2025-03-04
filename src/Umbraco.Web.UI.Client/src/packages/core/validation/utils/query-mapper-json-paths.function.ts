import { GetValueByJsonPath } from './json-path.function.js';
import { umbScopeMapperForJsonPaths } from './scope-mapper-json-paths.function.js';

/**
 * Helper method to replace paths index with queries and additionally map the paths.
 * @param {Array<string>} paths - the JSON paths to map.
 * @param {string} scopePath - the JSON path to scope the mapping to.
 * @param {(Array<string>)} scopedMapper - Map function which receives the paths in the scope and returns the resolved paths.
 * @returns {string} - the paths, including the once mapped by the scoped mapper. Notice the order is kept.
 */
export async function umbQueryMapperForJsonPaths<T>(
	scopePaths: Array<string>,
	scopeData: Array<T>,
	queryConstructor: (entry: T) => string,
	mapper?: (scopedPaths: Array<string>, propertyData: T) => Promise<Array<string>>,
): Promise<Array<string>> {
	const uniquePointers: Array<string> = [];

	// Make sure each entry gets a query:
	// propertyPaths is like ['$[0].value', '$[0].value.something', '$[1].value', '$[?.(@.prop == 'value')].value'] group them by the first index or query.:
	let pathsWithQueries = scopePaths.map(
		(path) => {
			if (!path.startsWith('$[')) {
				throw new Error('Invalid JSON-Path query `' + path + '`. Expected to start with `$[`.');
			}
			// This could already be translated, meaning it can both be an index or a JSON-Path query.
			const index = path.indexOf(']');
			// grab everything between `$[` and `]`
			let pointer = path.substring(2, index);
			const afterQuery = path.substring(index + 1);
			const numberPointer = Number(pointer);

			// If a number index, then create JSON-Path Query:
			if (!isNaN(numberPointer)) {
				pointer = queryConstructor(scopeData[numberPointer]);
			}

			if (!uniquePointers.includes(pointer)) {
				uniquePointers.push(pointer);
			}

			return `$[${pointer}]${afterQuery}`;
		},
		{} as Record<string, Array<string>>,
	);

	if (mapper) {
		// map each property:
		for (const uniquePath of uniquePointers) {
			const propertyData = GetValueByJsonPath(scopeData, `$[${uniquePath}]`) as T;

			pathsWithQueries = await umbScopeMapperForJsonPaths(
				pathsWithQueries,
				`$[${uniquePath}]`,
				async (scopedPaths: Array<string>) => {
					return mapper(scopedPaths, propertyData);
				},
			);
		}
	}
	return pathsWithQueries;
}
