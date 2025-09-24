import type { UmbPropertyEditorDataSource } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyEditorDataSource extends ManifestApi<UmbPropertyEditorDataSource> {
	type: 'propertyEditorDataSource';
	meta: MetaPropertyEditorDataSource;
}

export interface MetaPropertyEditorDataSource {
	label: string;
	icon: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorDataSource: ManifestPropertyEditorDataSource;
	}
}
