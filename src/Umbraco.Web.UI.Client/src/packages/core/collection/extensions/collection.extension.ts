import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollection
	extends ManifestElementAndApi,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collection';
	meta: MetaCollection;
}

export interface MetaCollection {
	repositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollection: ManifestCollection;
	}
}
