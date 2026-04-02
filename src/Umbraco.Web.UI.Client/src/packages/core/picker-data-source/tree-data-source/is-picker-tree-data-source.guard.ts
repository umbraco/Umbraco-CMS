import type { UmbPickerTreeDataSource } from './types.js';

/**
 *
 * @param dataSource
 */
export function isPickerTreeDataSource(dataSource: unknown): dataSource is UmbPickerTreeDataSource {
	return (
		(dataSource as UmbPickerTreeDataSource).requestTreeRoot !== undefined &&
		(dataSource as UmbPickerTreeDataSource).requestTreeRootItems !== undefined &&
		(dataSource as UmbPickerTreeDataSource).requestTreeItemsOf !== undefined
	);
}
