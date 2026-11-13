import type { UmbFilterExtensionApi } from './filter-extension-api.interface.js';
import type { UmbFilterExtensionElement } from './filter-extension-element.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaFilter {
	label: string;
}

/**
 * The manifest shape shared by the filter extension types of every filter host. A host declares its own extension
 * type on top of it, for example `collectionFilter`, which scopes filters to collections with `forCollections`.
 */
export interface ManifestFilterBase<
	ElementType extends UmbFilterExtensionElement<any> = UmbFilterExtensionElement<any>,
	ApiType extends UmbFilterExtensionApi<any> = UmbFilterExtensionApi<any>,
>
	extends ManifestElementAndApi<ElementType, ApiType>, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	meta: MetaFilter;
}
