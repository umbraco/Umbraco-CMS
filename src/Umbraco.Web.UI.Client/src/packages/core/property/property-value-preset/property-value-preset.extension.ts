import type { UmbPropertyValueResolver } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValuePreset extends ManifestApi<UmbPropertyValueResolver<any>> {
	type: 'propertyValuePreset';
	forEditorAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValuePreset: ManifestPropertyValuePreset;
	}
}
