import type { ManifestElement, ApiLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import type { UmbValueSummaryElementInterface } from './value-summary-element.interface.js';
import type { UmbValueSummaryApi } from './value-summary-api.interface.js';

export interface ManifestValueSummary extends ManifestElement<UmbValueSummaryElementInterface> {
	type: 'valueSummary';
	/**
	 * The value type this summary is registered for.
	 */
	forValueType: keyof UmbValueTypeMap;
	/**
	 * Optional API for batch-resolving raw values to display values before rendering.
	 */
	api?: ApiLoaderProperty<UmbValueSummaryApi>;
	meta: MetaValueSummary;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaValueSummary {}

declare global {
	interface UmbExtensionManifestMap {
		umbValueSummary: ManifestValueSummary;
	}
}
