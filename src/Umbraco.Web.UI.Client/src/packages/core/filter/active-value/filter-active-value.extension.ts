import type { UmbFilterValueResolverLoaderProperty } from '../value-resolver/filter-value-resolver.interface.js';
import type { UmbFilterActiveValueApi } from './filter-active-value-api.interface.js';
import type { UmbFilterActiveValueElement } from './filter-active-value-element.interface.js';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Displays the active values of one or more filters, for example as removable chips above a collection.
 * Without a matching registration, an active value is displayed by the `default` kind.
 */
export interface ManifestFilterActiveValue extends ManifestElementAndApi<
	UmbFilterActiveValueElement,
	UmbFilterActiveValueApi
> {
	type: 'filterActiveValue';
	/**
	 * The aliases of the filters whose active values this extension displays.
	 */
	forFilters: Array<string>;
	/**
	 * Optional resolver for the active values before they are displayed, for example from IDs to items.
	 * Created once per filter by the filter value coordinator, so values are batched across all displayed
	 * values of the filter. Without a resolver, the element receives the raw value.
	 */
	valueResolver?: UmbFilterValueResolverLoaderProperty;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFilterActiveValue: ManifestFilterActiveValue;
	}
}
