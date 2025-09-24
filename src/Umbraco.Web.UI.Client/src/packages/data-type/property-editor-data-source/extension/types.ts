import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './picker-property-editor-collection-data-source.extension.js';
export type * from './picker-property-editor-tree-data-source.extension.js';

export interface UmbPickerPropertyEditorTreeDataSource extends UmbApi {
	execute(): Promise<void>;
}

export interface UmbPickerPropertyEditorCollectionDataSource extends UmbApi {
	execute(): Promise<void>;
}
