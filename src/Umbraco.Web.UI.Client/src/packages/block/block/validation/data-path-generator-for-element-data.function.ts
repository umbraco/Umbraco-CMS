import type { UmbBlockDataModel } from '../types.js';
import { UmbDataPathBlockElementDataQuery } from './data-path-element-data-query.function.js';

/**
 * Validation Data Path generator for Block Element Data.
 * Writes a JSON-Path similar to `$.contentData[?(@.key == 'my-key://1234')]`
 * @param {string} dataPathPropertyName - The property name holding the element data, either `contentData` or `settingsData`.
 * @param {{key: string}} data - A data object with the key property.
 * @returns {string} The JSON-Path.
 */
export function UmbDataPathGeneratorForBlockElementData(
	dataPathPropertyName: string,
	data: Pick<UmbBlockDataModel, 'key'>,
): string {
	return `$.${dataPathPropertyName}[${UmbDataPathBlockElementDataQuery(data)}]`;
}
