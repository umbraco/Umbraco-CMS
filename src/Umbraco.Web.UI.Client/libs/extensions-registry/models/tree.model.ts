import type { ManifestBase } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestTree extends ManifestBase {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	repositoryAlias: string;
}
