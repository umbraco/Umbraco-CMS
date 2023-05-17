import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionView extends ManifestElement, ManifestWithConditions<ConditionsCollectionView> {
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
	 *
	 * @examples [
	 *   "umb:box",
	 *   "umb:grid"
	 * ]
	 */
	icon: string;

	/**
	 * The URL pathname for this collection view that can be deep linked to by sharing the url
	 */
	pathName: string;
}

export interface ConditionsCollectionView {
	entityType: string;
}
