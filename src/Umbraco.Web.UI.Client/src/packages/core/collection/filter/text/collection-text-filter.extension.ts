import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionTextFilter
	extends ManifestElement,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionTextFilter';
	meta: MetaCollectionTextFilter;
}

export interface MetaCollectionTextFilter {}

declare global {
	interface UmbExtensionManifestMap {
		collectionTextFilter: ManifestCollectionTextFilter;
	}
}
