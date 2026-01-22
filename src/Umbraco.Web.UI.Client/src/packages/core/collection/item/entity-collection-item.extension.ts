import type { UmbEntityCollectionItemElement } from './entity-collection-item-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * Base interface for entity collection item manifests.
 * Shared by card and ref variants.
 */
export interface ManifestEntityCollectionItemBase<MetaType = object>
	extends ManifestElement<UmbEntityCollectionItemElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	/**
	 * The entity types this collection item supports.
	 */
	forEntityTypes: Array<string>;

	/**
	 * Additional metadata for the collection item.
	 */
	meta?: MetaType;
}
