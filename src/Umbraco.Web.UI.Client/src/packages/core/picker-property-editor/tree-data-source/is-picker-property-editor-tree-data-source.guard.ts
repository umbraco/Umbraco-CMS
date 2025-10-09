import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import type { UmbPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';

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
