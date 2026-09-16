import type { UmbBlockDataModel } from '../types.js';

/**
 * Validation Data Path Query generator for Block Element Data.
 * write a JSON-Path filter similar to `?(@.key == 'my-key://1234')`
 * @param {{key: string}} data - A data object with the key property.
 * @returns {string} The JSON-Path filter query.
 */
export function UmbDataPathBlockElementDataQuery(data: Pick<UmbBlockDataModel, 'key'>): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	//const filters: Array<string> = [`@.key == '${key}'`];
	//return `?(${filters.join(' && ')})`;
	return `?(@.key == '${data.key}')`;
}
