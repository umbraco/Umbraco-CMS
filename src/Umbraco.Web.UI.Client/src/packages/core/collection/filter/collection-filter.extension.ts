import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionFilter
	extends ManifestElementAndApi<any, any>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionFilter';
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionFilter: ManifestCollectionFilter;
	}
}
