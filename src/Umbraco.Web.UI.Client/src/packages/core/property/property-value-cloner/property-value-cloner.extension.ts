import type { UmbPropertyValueCloner } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueCloner extends ManifestApi<UmbPropertyValueCloner<any>> {
	type: 'propertyValueCloner';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueTransformer: ManifestPropertyValueCloner;
	}
}
