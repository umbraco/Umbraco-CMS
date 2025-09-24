import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPickerPropertyEditorCollectionDataSource
	extends ManifestApi<UmbPickerPropertyEditorCollectionDataSource> {
	type: 'pickerPropertyEditorCollectionDataSource';
	meta: MetaPickerPropertyEditorCollectionDataSource;
}

export interface MetaPickerPropertyEditorCollectionDataSource {
	description: string;
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPickerPropertyEditorCollectionDataSource: ManifestPickerPropertyEditorCollectionDataSource;
	}
}
