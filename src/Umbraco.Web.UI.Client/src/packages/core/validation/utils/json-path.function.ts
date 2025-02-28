/**
 *
 * @param {object} data - object to traverse for the value.
 * @param {string} path - the JSON path to the value that should be found
 * @returns {unknown} - the found value.
 */
export function GetValueByJsonPath(data: unknown, path: string): unknown {
	if (path === '$') return data;
	// strip $ from the path:
	if (path.startsWith('$[')) {
		return _GetNextArrayEntryFromPath(data as Array<unknown>, path.slice(2));
	}

	const strippedPath = path.startsWith('$.') ? path.slice(2) : path;
	// get value from the path:
	return GetNextPropertyValueFromPath(data, strippedPath);
}

/**
 *
 * @param {object} data - object to traverse for the value.
 * @param {string} path - the JSON path to the value that should be found
 * @returns {unknown} - the found value.
 */
function GetNextPropertyValueFromPath(data: any, path: string): any {
	if (!data) return undefined;
	// find next '.' or '[' in the path, using regex:
	const match = path.match(/\.|\[/);
	// If no match is found, we assume its a single key so lets return the value of the key:
	if (match === null || match.index === undefined) return data[path];

	// split the path at the first match:
	const key = path.slice(0, match.index);
	const rest = path.slice(match.index + 1);

	if (!key) return undefined;
	// get the value of the key from the data:
	const value = data[key];
	// if there is no rest of the path, return the value:
	if (rest === undefined) return value;

	// if the value is an array, get the value at the index:
	if (Array.isArray(value)) {
		return _GetNextArrayEntryFromPath(value, rest);
	} else {
		// continue with the rest of the path:
		return GetNextPropertyValueFromPath(value, rest);
	}
}

/**
 * @private
 * @param {object} array - object to traverse for the value.
 * @param {string} path - the JSON path to the value that should be found, notice without the starting '['
 * @returns {unknown} - the found value.
 */
function _GetNextArrayEntryFromPath(array: Array<any>, path: string): any {
	if (!array) return undefined;

	// get the value until the next ']', the value can be anything in between the brackets:
	const lookupEnd = path.match(/\]/);
	if (!lookupEnd) return undefined;
	// get everything before the match:
	const entryPointer = path.slice(0, lookupEnd.index);

	// check if the entryPointer is a JSON Path Filter ( starting with ?( and ending with ) ):
	if (entryPointer.startsWith('?(') && entryPointer.endsWith(')')) {
		// get the filter from the entryPointer:
		// get the filter as a function:
		const jsFilter = JsFilterFromJsonPathFilter(entryPointer);
		// find the index of the value that matches the filter:
		const index = array.findIndex(jsFilter[0]);
		// if the index is -1, return undefined:
		if (index === -1) return undefined;
		// get the value at the index:
		const entryData = array[index];
		// Check for safety:
		if (lookupEnd.index === undefined || lookupEnd.index + 1 >= path.length) {
			return entryData;
		}
		// continue with the rest of the path:
		return GetNextPropertyValueFromPath(entryData, path.slice(lookupEnd.index + 2)) ?? entryData;
	} else {
		// get the value at the index:
		const indexAsNumber = parseInt(entryPointer);
		if (isNaN(indexAsNumber)) return undefined;
		const entryData = array[indexAsNumber];
		// Check for safety:
		if (lookupEnd.index === undefined || lookupEnd.index + 1 >= path.length) {
			return entryData;
		}
		// continue with the rest of the path:
		return GetNextPropertyValueFromPath(entryData, path.slice(lookupEnd.index + 2)) ?? entryData;
	}
}

/**
 * @param {string} filter - A JSON Query, limited to filtering features. Do not support other JSON PATH Query features.
 * @returns {Array<(queryFilter: any) => boolean>} - An array of methods that returns true if the given items property value matches the value of the query.
 */
function JsFilterFromJsonPathFilter(filter: string): Array<(item: any) => boolean> {
	// strip ?( and ) from the filter
	const jsFilter = filter.slice(2, -1);
	// split the filter into parts by splitting at ' && '
	const parts = jsFilter.split(' && ');
	// map each part to a function that returns true if the part is true
	return parts.map((part) => {
		// split the part into key and value
		const [path, equal] = part.split(' == ');
		// remove @.
		const key = path.slice(2);
		// remove quotes:
		const value = equal.slice(1, -1);
		// return a function that returns true if the key is equal to the value
		return (item: any) => item[key] === value;
	});
}
