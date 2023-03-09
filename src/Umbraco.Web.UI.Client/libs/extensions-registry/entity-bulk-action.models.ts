import type { ManifestElement } from './models';

export interface ManifestEntityBulkAction extends ManifestElement {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
}

export interface MetaEntityBulkAction {
	label: string;
	entityType: string;
	api: any; // create interface
	repositoryAlias: string;
}
