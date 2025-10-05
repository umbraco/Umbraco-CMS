import type { UmbPickerPropertyEditorTreeDataSource, UmbPropertyEditorDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerPropertyEditorTreeDataSource(
	dataSource: UmbPropertyEditorDataSource,
): dataSource is UmbPickerPropertyEditorTreeDataSource {
	return (
		(dataSource as UmbPickerPropertyEditorTreeDataSource).requestTreeRoot !== undefined &&
		(dataSource as UmbPickerPropertyEditorTreeDataSource).requestTreeRootItems !== undefined &&
		(dataSource as UmbPickerPropertyEditorTreeDataSource).requestTreeItemsOf !== undefined
	);
}
