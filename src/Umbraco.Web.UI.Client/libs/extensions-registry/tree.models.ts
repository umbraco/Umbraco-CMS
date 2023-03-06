import type { ClassConstructor, ManifestBase } from './models';

export interface ManifestTree extends ManifestBase {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	storeAlias?: string;
	repository?: ClassConstructor<unknown>;
}
