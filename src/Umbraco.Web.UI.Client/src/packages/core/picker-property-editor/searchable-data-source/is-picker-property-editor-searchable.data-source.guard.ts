import type { UmbPickerPropertyEditorSearchableDataSource } from './types.js';
import type { UmbPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';

/**
 *
 * @param dataSource
 */
export function isPickerPropertyEditorSearchableDataSource(
	dataSource: UmbPropertyEditorDataSource,
): dataSource is UmbPickerPropertyEditorSearchableDataSource {
	return (dataSource as UmbPickerPropertyEditorSearchableDataSource).search !== undefined;
}
