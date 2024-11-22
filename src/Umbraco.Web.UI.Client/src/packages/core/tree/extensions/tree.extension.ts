import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTree
	extends ManifestElementAndApi,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	repositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTree: ManifestTree;
	}
}
