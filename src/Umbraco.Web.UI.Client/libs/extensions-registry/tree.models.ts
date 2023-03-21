import type { ManifestBase } from './models';
import type { ClassConstructor } from '@umbraco-cms/backoffice/models';

export interface ManifestTree extends ManifestBase {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	storeAlias?: string;
	repository?: ClassConstructor<unknown>;
}
