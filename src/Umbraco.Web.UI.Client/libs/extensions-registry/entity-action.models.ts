import type { ManifestElement } from './models';

export interface ManifestEntityAction extends ManifestElement {
	type: 'entityAction';
	meta: MetaEntityAction;
	conditions: ConditionsEntityAction;
}

export interface MetaEntityAction {
	icon?: string;
	label: string;
	api: any; // create interface
	repositoryAlias: string;
}

export interface ConditionsEntityAction {
	entityType: string;
}
