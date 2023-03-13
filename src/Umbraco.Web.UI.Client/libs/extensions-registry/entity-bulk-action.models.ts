import type { ManifestElement } from './models';

export interface ManifestEntityBulkAction extends ManifestElement {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
	conditions: ConditionsEntityBulkAction;
}

export interface MetaEntityBulkAction {
	label: string;
	api: any; // create interface
	repositoryAlias: string;
}

export interface ConditionsEntityBulkAction {
	entityType: string;
}
