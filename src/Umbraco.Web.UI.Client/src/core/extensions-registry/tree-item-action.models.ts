import type { ManifestElement } from './models';

export interface ManifestTreeItemAction extends ManifestElement {
	type: 'treeItemAction';
	meta: MetaTreeItemAction;
}

export interface MetaTreeItemAction {
	entityType: string;
	label: string;
	icon: string;
}
