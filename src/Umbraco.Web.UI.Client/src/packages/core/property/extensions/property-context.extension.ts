import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyContext
	extends ManifestWithDynamicConditions<UmbExtensionConditionConfig>,
		ManifestApi<UmbApi> {
	type: 'propertyContext';
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestPropertyContext: ManifestPropertyContext;
	}
}
