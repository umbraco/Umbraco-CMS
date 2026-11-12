import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaFilter {
	label: string;
}

export interface ManifestFilter
	extends ManifestElementAndApi<any, any>, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'filter';
	meta: MetaFilter;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFilter: ManifestFilter;
	}
}
