import type { ManifestElement } from './models';

export interface ManifestTreeItemAction extends ManifestElement {
	type: 'treeItemAction';
	meta: MetaTreeItemAction;
}

export interface MetaTreeItemAction {
	trees: Array<string>;
	label: string;
	icon: string;
}
