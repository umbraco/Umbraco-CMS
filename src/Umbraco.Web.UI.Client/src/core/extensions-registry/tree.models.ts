import type { ManifestElement } from './models';

export interface ManifestTree extends ManifestElement {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	sections: Array<string>;
}
