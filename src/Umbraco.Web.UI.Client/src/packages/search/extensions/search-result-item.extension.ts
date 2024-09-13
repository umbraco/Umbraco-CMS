import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

/**
 * Represents a search result element.
 */
export interface ManifestSearchResultItem extends ManifestElement {
	type: 'searchResultItem';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbSearchResultItem: ManifestSearchResultItem;
	}
}
