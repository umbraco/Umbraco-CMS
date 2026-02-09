import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestCollectionView
	extends ManifestElement,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
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
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon: string;

	/**
	 * The URL pathname for this collection view that can be deep linked to by sharing the url
	 */
	pathName: string;
}

/**
 * Condition for when this collection view should be available
 */
export interface UmbConditionsCollectionView {
	/**
	 * Type of entity this collection view should be available for
	 * @examples ["media"]
	 */
	entityType: string;
}

/**
 * @deprecated Use {@link UmbConditionsCollectionView} instead. This will be removed in Umbraco 18.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/naming-convention
export interface ConditionsCollectionView extends UmbConditionsCollectionView {}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionView: ManifestCollectionView;
	}
}
