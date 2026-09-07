import type { UmbPickerTreeDataSource } from './types.js';

/**
 * Checks if the given data source is a tree data source.
 * @param {unknown} dataSource - The data source to check
 * @returns {boolean} True if the data source is a tree data source
 */
export function isPickerTreeDataSource(dataSource: unknown): dataSource is UmbPickerTreeDataSource {
	return (
		(dataSource as UmbPickerTreeDataSource).requestTreeRoot !== undefined &&
		(dataSource as UmbPickerTreeDataSource).requestTreeRootItems !== undefined &&
		(dataSource as UmbPickerTreeDataSource).requestTreeItemsOf !== undefined
	);
}
