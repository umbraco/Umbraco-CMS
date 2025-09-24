import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbPickerPropertyEditorTreeDataSource extends UmbApi {
	execute(): Promise<void>;
}

export interface UmbPickerPropertyEditorCollectionDataSource extends UmbApi {
	execute(): Promise<void>;
}
