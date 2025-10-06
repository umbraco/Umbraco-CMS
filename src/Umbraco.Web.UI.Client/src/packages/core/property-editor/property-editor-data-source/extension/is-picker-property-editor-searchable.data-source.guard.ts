import type { UmbPickerPropertyEditorSearchableDataSource, UmbPropertyEditorDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerPropertyEditorSearchableDataSource(
	dataSource: UmbPropertyEditorDataSource,
): dataSource is UmbPickerPropertyEditorSearchableDataSource {
	return (dataSource as UmbPickerPropertyEditorSearchableDataSource).search !== undefined;
}
