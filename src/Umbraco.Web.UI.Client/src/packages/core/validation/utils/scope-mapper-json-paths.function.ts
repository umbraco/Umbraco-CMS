/**
 * Helper method to map paths matching a certain scope.
 * @param {Array<string>} paths - the JSON paths to map.
 * @param {string} scopePath - the JSON path to scope the mapping to.
 * @param {(Array<string>)} scopedMapper - Map function which receives the paths in the scope and returns the resolved paths.
 * @returns {string} - the paths, including the once mapped by the scoped mapper. Notice the order is kept.
 */
export async function umbScopeMapperForJsonPaths(
	paths: Array<string>,
	scopePath: string,
	scopedMapper: (scopedPaths: Array<string>) => Promise<Array<string>>,
): Promise<Array<string>> {
	const basePathLength = scopePath.length;
	const inScope: Array<string> = [];
	const inScopeIndexes: Array<number> = [];
	for (let i = 0; i < paths.length; i++) {
		if (paths[i].indexOf(scopePath) === 0) {
			inScopeIndexes.push(i);
			inScope.push('$' + paths[i].substring(basePathLength));
		}
	}
	paths = [...paths];
	// Map resolved paths to the original position:
	const resolvedPaths = await scopedMapper(inScope);
	for (let i = 0; i < resolvedPaths.length; i++) {
		paths[inScopeIndexes[i]] = scopePath + resolvedPaths[i].substring(1);
	}

	return paths;
}
