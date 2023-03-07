import type { ManifestElement } from './models';

/**
 * A collection view is a view that can be used to display a collection of entities.
 * For example you may wish to display a collection of nodes as a table or grid of cards
 */
export interface ManifestCollectionView extends ManifestElement {
	type: 'collectionView';
	meta: MetaCollectionView;
}

export interface MetaCollectionView {
	/**
	 * The friendly name of the collection view
	 */
	label: string;

	/**
	 * An icon to represent the collection view
	 * @example 'umb:box'
	 * @example 'umb:grid'
	 */
	icon: string;

	/**
	 * The entity type that this collection view is for
	 * @example 'media'
	 */
	entityType: string;

	/**
	 * The URL pathname for this collection view that can be deep linked to by sharing the url
	 */
	pathName: string;
}
