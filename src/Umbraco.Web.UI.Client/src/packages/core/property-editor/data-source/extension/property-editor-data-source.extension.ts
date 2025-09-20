import type { UmbPropertyEditorDataSource } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyEditorDataSource extends ManifestApi<UmbPropertyEditorDataSource> {
	type: 'propertyEditorDataSource';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorDataSource: UmbPropertyEditorDataSource;
	}
}
