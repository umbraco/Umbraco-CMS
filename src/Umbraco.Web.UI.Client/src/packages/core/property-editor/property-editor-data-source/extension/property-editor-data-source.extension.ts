import type { PropertyEditorSettings } from '../../extensions/types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: base ManifestApiType on dataSourceType
export interface ManifestPropertyEditorDataSource extends ManifestApi<any> {
	type: 'propertyEditorDataSource';
	dataSourceType: string;
	meta: MetaPropertyEditorDataSource;
}

export interface MetaPropertyEditorDataSource {
	description: string;
	icon: string;
	label: string;
	settings?: PropertyEditorSettings;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorDataSource: ManifestPropertyEditorDataSource;
	}
}
