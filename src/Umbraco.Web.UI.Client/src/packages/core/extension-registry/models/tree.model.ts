import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTree extends ManifestElementAndApi {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	repositoryAlias: string;
}
