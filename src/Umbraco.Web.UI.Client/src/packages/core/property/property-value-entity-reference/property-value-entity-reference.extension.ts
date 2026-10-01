import type { UmbPropertyValueEntityReferenceResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueEntityReference
	extends ManifestApi<UmbPropertyValueEntityReferenceResolver> {
	type: 'propertyValueEntityReference';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueEntityReference: ManifestPropertyValueEntityReference;
	}
}
