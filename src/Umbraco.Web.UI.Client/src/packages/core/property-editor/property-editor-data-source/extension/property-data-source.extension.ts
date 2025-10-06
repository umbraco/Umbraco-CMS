import type { UmbPropertyEditorDataSource } from '../types.js';
import type { PropertyEditorSettings } from '@umbraco-cms/backoffice/property-editor';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyEditorDataSource extends ManifestApi<UmbPropertyEditorDataSource> {
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
