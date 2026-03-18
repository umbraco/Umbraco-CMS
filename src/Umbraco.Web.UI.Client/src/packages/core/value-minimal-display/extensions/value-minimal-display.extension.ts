import type { ManifestElement, ApiLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import type { UmbValueMinimalDisplayElementInterface } from './value-minimal-display-element.interface.js';
import type { UmbValueMinimalDisplayApi } from './value-minimal-display-api.interface.js';

export interface ManifestValueMinimalDisplay extends ManifestElement<UmbValueMinimalDisplayElementInterface> {
	type: 'valueMinimalDisplay';
	/**
	 * Optional API for batch-resolving raw values to display values before rendering.
	 */
	api?: ApiLoaderProperty<UmbValueMinimalDisplayApi>;
	meta: MetaValueMinimalDisplay;
}

export interface MetaValueMinimalDisplay {
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbValueMinimalDisplay: ManifestValueMinimalDisplay;
	}
}
