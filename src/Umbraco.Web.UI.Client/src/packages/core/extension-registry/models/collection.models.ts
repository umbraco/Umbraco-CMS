import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollection
	extends ManifestElementAndApi,
		ManifestWithDynamicConditions<UmbExtensionCondition> {
	type: 'collection';
	meta: MetaCollection;
}

export interface MetaCollection {
	repositoryAlias: string;
}
