import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestCollectionView extends ManifestElement, ManifestWithConditions<ConditionsCollectionView> {
	type: 'collectionView';
	meta: MetaCollectionView;
}

export interface MetaCollectionView {
	label: string;
	icon: string;
	pathName: string;
}

export interface ConditionsCollectionView {
	entityType: string;
}
