import type { ManifestElement } from './models';

export interface ManifestEntityAction extends ManifestElement {
	type: 'entityAction';
	meta: MetaEntityAction;
}

export interface MetaEntityAction {
	icon: string;
	label: string;
	entityType: string;
	api: any; // create interface
}
