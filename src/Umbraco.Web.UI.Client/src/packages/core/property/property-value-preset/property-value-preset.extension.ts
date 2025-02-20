import type { UmbPropertyValuePresetApi } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyValuePreset extends ManifestApi<UmbPropertyValuePresetApi<any>> {
	type: 'propertyValuePreset';
	forPropertyEditorSchemaAlias?: string;
	forPropertyEditorUiAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyValuePreset: ManifestPropertyValuePreset;
	}
}
