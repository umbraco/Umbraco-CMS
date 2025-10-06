import type { UmbPickerPropertyEditorCollectionDataSource, UmbPropertyEditorDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerPropertyEditorCollectionDataSource(
	dataSource: UmbPropertyEditorDataSource,
): dataSource is UmbPickerPropertyEditorCollectionDataSource {
	return (dataSource as UmbPickerPropertyEditorCollectionDataSource).requestCollection !== undefined;
}
