import type { ManifestElement } from './models';

export interface ManifestCollectionView extends ManifestElement {
	type: 'collectionView';
	meta: MetaCollectionView;
}

export interface MetaCollectionView {
	label: string;
	icon: string;
	entityType: string;
	pathName: string;
}
