import type { UmbSearchProvider, UmbSearchResultItemModel } from '../types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Represents an search provider that can be used to search.
 */
export interface ManifestSearchProvider extends ManifestApi<UmbSearchProvider<UmbSearchResultItemModel>> {
	type: 'searchProvider';

	meta?: MetaSearchProvider;
}

export interface MetaSearchProvider {
	/**
	 * The label of the provider that is shown to the user.
	 */
	label?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbSearchProvider: ManifestSearchProvider;
	}
}
