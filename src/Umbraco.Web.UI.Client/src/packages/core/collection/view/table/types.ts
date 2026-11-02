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
	/** Property path on the collection item model to display in this column. Supports dot-notation for nested fields (e.g. `author.name`). */
	field: string;
	/** The column header label. Supports localization strings (e.g. `#general_status`). */
	label: string;
	/** Optional value type for rendering a value summary in this column. */
	valueType?: keyof UmbValueTypeMap;
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
