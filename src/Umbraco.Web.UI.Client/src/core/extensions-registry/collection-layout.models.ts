import type { ManifestElement } from './models';

export interface ManifestCollectionLayout extends ManifestElement {
	type: 'collectionLayout';
	meta: MetaCollectionLayout;
}

export interface MetaCollectionLayout {
	label: string;
	icon: string;
	entityType: string;
	pathName: string;
}
