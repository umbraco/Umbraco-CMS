import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Represents a picker search result element.
 */
export interface ManifestPickerSearchResultItem extends ManifestElementAndApi {
	type: 'pickerSearchResultItem';
	forEntityTypes: Array<string>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbPickerSearchResultItem: ManifestPickerSearchResultItem;
	}
}
