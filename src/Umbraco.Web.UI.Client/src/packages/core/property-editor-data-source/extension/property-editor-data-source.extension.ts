import type { PropertyEditorSettings } from '@umbraco-cms/backoffice/property-editor';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: base ManifestApiType on dataSourceType
export interface ManifestPropertyEditorDataSource extends ManifestApi<any> {
	type: 'propertyEditorDataSource';
	dataSourceType: string;
	meta: MetaPropertyEditorDataSource;
}

export interface MetaPropertyEditorDataSource {
	label: string;
	description?: string;
	icon?: string;
	settings?: PropertyEditorSettings;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorDataSource: ManifestPropertyEditorDataSource;
	}
}
