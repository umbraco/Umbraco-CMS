import type { ManifestBase } from './models';
import type { UmbRepositoryFactory } from '@umbraco-cms/models';

export interface ManifestTree extends ManifestBase {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	storeAlias?: string;
	repository?: UmbRepositoryFactory;
}
