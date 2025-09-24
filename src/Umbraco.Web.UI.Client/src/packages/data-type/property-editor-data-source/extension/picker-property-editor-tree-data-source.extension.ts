import type { UmbPickerPropertyEditorTreeDataSource } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPickerPropertyEditorTreeDataSource extends ManifestApi<UmbPickerPropertyEditorTreeDataSource> {
	type: 'pickerPropertyEditorTreeDataSource';
	meta: MetaPickerPropertyEditorTreeDataSource;
}

export interface MetaPickerPropertyEditorTreeDataSource {
	description: string;
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPickerPropertyEditorTreeDataSource: ManifestPickerPropertyEditorTreeDataSource;
	}
}
