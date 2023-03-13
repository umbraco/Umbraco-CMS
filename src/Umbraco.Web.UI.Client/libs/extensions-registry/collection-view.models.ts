import type { ManifestElement } from './models';

export interface ManifestCollectionView extends ManifestElement {
	type: 'collectionView';
	meta: MetaCollectionView;
	conditions: ConditionsCollectionView;
}

export interface MetaCollectionView {
	label: string;
	icon: string;
	pathName: string;
}

export interface ConditionsCollectionView {
	entityType: string;
}
