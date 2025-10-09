import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import type { UmbPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';

/**
 *
 * @param dataSource
 */
export function isPickerPropertyEditorCollectionDataSource(
	dataSource: UmbPropertyEditorDataSource,
): dataSource is UmbPickerPropertyEditorCollectionDataSource {
	return (dataSource as UmbPickerPropertyEditorCollectionDataSource).requestCollection !== undefined;
}
