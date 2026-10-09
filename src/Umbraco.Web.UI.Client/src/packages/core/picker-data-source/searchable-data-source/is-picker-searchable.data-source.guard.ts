import type { UmbPickerSearchableDataSource } from './types.js';

/**
 * Checks if the given data source is a searchable data source.
 * @param {unknown} dataSource - The data source to check
 * @returns {boolean} True if the data source is a searchable data source
 */
export function isPickerSearchableDataSource(dataSource: unknown): dataSource is UmbPickerSearchableDataSource {
	return (dataSource as UmbPickerSearchableDataSource).search !== undefined;
}
