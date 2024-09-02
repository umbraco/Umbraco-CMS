import type { UmbBlockDataModel } from '../types.js';

/**
 * Validation Data Path Query generator for Block Element Data.
 * write a JSON-Path filter similar to `?(@.udi = 'my-udi://1234')`
 * @param udi {string} - The udi of the block Element data.
 * @param data {{udi: string}} - A data object with the udi property.
 * @returns
 */
export function UmbDataPathBlockElementDataQuery(data: Pick<UmbBlockDataModel, 'udi'>): string {
	// write a array of strings for each property, where alias must be present and culture and segment are optional
	//const filters: Array<string> = [`@.udi = '${udi}'`];
	//return `?(${filters.join(' && ')})`;
	return `?(@.udi = '${data.udi}')`;
}
