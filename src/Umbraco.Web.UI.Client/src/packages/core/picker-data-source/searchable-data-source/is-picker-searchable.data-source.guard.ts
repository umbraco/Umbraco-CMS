import type { UmbPickerSearchableDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerSearchableDataSource(dataSource: unknown): dataSource is UmbPickerSearchableDataSource {
	return (dataSource as UmbPickerSearchableDataSource).search !== undefined;
}
