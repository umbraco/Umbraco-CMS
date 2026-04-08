import type { ManifestCollectionView, MetaCollectionView } from '../collection-view.extension.js';

/**
 * Manifest type for the `table` kind of `collectionView`.
 * Allows registering a table collection view with custom columns via manifest meta.
 */
export interface ManifestCollectionViewTableKind extends Omit<ManifestCollectionView, 'meta'> {
	type: 'collectionView';
	kind: 'table';
	meta: MetaCollectionViewTableKind;
}

/**
 * Configuration for a single column in a table collection view kind.
 */
export interface MetaCollectionViewTableKindColumn {
	/** The property name on the collection item model to display in this column. */
	field: string;
	/** The column header label. Supports localization strings (e.g. `#general_status`). */
	label: string;
	valueMinimalDisplayAlias?: string;
}

/**
 * Meta configuration for the `table` kind of `collectionView`.
 */
export interface MetaCollectionViewTableKind extends Partial<MetaCollectionView> {
	/** Additional columns to render between the name and entity actions columns. */
	columns?: Array<MetaCollectionViewTableKindColumn>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionViewTableKind: ManifestCollectionViewTableKind;
	}
}
