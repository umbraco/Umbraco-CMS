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
	/**
	 * The heading of the box. Supports localization keys, for example `#myPackage_myLabel`.
	 * The box element renders it, typically as the `headline` of its `uui-box`; the element
	 * receives its own manifest as `manifest`.
	 */
	label?: string;
	/** The column to render the box in. Defaults to `'right'`, the narrower sidebar column. */
	column?: 'left' | 'right';
}

declare global {
	interface UmbExtensionManifestMap {
		umbSearchIndexDetailBox: ManifestSearchIndexDetailBox;
	}
}
