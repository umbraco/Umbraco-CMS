import type { UmbPickerCollectionDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerCollectionDataSource(dataSource: unknown): dataSource is UmbPickerCollectionDataSource {
	return (dataSource as UmbPickerCollectionDataSource).requestCollection !== undefined;
}
