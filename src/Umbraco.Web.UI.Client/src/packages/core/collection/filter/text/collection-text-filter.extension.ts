import type { UmbCollectionTextFilterElement } from './collection-text-filter-element.interface.js';
import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionTextFilter
	extends ManifestElementAndApi<UmbCollectionTextFilterElement, UmbCollectionTextFilterApi>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionTextFilter';
	meta: MetaCollectionTextFilter;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaCollectionTextFilter {}

declare global {
	interface UmbExtensionManifestMap {
		collectionTextFilter: ManifestCollectionTextFilter;
	}
}
