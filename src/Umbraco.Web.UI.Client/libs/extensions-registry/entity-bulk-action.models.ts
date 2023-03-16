import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestEntityBulkAction extends ManifestElement, ManifestWithConditions<ConditionsEntityBulkAction> {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
}

export interface MetaEntityBulkAction {
	label: string;
	api: any; // create interface
	repositoryAlias: string;
}

export interface ConditionsEntityBulkAction {
	entityType: string;
}
