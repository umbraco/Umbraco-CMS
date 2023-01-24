import type { ManifestElement } from './models';

export interface ManifestCollectionBulkAction extends ManifestElement {
	type: 'collectionBulkAction';
	meta: MetaCollectionBulkAction;
}

export interface MetaCollectionBulkAction {
	label: string;
	entityType: string;
}
