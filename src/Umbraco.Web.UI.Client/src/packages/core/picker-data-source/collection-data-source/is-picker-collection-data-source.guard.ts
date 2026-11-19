import type { UmbPickerCollectionDataSource } from './types.js';

/**
 * Checks if the given data source is a collection data source.
 * @param {unknown} dataSource - The data source to check
 * @returns {boolean} True if the data source is a collection data source
 */
export function isPickerCollectionDataSource(dataSource: unknown): dataSource is UmbPickerCollectionDataSource {
	return (dataSource as UmbPickerCollectionDataSource).requestCollection !== undefined;
}
