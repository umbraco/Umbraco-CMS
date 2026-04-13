import type { ManifestElementAndApi, ApiLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import type { UmbValueSummaryElement } from './value-summary-element.interface.js';
import type { UmbValueSummaryApi } from './value-summary-api.interface.js';
import type { UmbValueSummaryResolver } from './value-summary-resolver.interface.js';

export interface ManifestValueSummary
	extends ManifestElementAndApi<UmbValueSummaryElement, UmbValueSummaryApi> {
	type: 'valueSummary';
	/**
	 * The value type this summary is registered for.
	 */
	forValueType: keyof UmbValueTypeMap;
	/**
	 * Optional resolver for batch-resolving raw values before rendering.
	 * Used by the coordinator for efficient batching across multiple elements.
	 */
	resolver?: ApiLoaderProperty<UmbValueSummaryResolver>;
	meta: MetaValueSummary;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaValueSummary {}

declare global {
	interface UmbExtensionManifestMap {
		umbValueSummary: ManifestValueSummary;
	}
}
