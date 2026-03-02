import type { ManifestCollectionView, MetaCollectionView } from '../collection-view.extension.js';

export interface ManifestCollectionViewTableKind extends ManifestCollectionView {
	type: 'collectionView';
	kind: 'table';
	meta: MetaCollectionViewTableKind;
}

export interface MetaCollectionViewTableKindColumn {
	field: string;
	headerName: string;
}

export interface MetaCollectionViewTableKind extends MetaCollectionView {
	columns?: Array<MetaCollectionViewTableKindColumn>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionViewTableKind: ManifestCollectionViewTableKind;
	}
}
