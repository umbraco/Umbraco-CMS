import type { UmbPropertyValueTransformer } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValueTransformer extends ManifestApi<UmbPropertyValueTransformer<any>> {
	type: 'propertyValueTransformer';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValueTransformer: ManifestPropertyValueTransformer;
	}
}
