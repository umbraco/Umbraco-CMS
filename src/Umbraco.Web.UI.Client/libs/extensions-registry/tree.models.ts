import type { ManifestBase } from './models';

export interface ManifestTree extends ManifestBase {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	storeAlias?: string;
	repository?: any; // TODO: add interface
}
