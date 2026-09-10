import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * A box rendered in the search index detail workspace view.
 *
 * A search provider can contribute its own boxes (e.g. provider-specific statistics or actions)
 * by registering an extension of this type.
 */
export interface ManifestSearchIndexDetailBox
	extends ManifestElement, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'searchIndexDetailBox';
	meta?: MetaSearchIndexDetailBox;
}

export interface MetaSearchIndexDetailBox {
	label?: string;
	column?: 'left' | 'right';
}

declare global {
	interface UmbExtensionManifestMap {
		umbSearchIndexDetailBox: ManifestSearchIndexDetailBox;
	}
}
