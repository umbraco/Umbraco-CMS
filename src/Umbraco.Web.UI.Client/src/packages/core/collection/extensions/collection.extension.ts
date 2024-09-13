import type { ConditionTypes } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollection extends ManifestElementAndApi, ManifestWithDynamicConditions<ConditionTypes> {
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
