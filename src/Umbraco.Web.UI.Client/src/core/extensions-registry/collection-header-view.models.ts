import type { ManifestElement } from './models';

export interface ManifestCollectionHeaderView extends ManifestElement {
	type: 'collectionHeaderView';
	meta: MetaCollectionHeaderView;
}

export interface MetaCollectionHeaderView {
	label: string;
	icon: string;
	entityType: string;
}
