import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import type { ManifestCollectionTextFilter } from './collection-text-filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * The element of a `collectionTextFilter` extension.
 *
 * Render a `filters` slot where controls that refine the collection alongside the text belong. The collection toolbar
 * places the button that opens the filters in it; without the slot, that button is not shown.
 * @slot filters - Controls that refine the collection alongside the text, for example a button that opens the filters.
 */
export interface UmbCollectionTextFilterElement extends UmbControllerHostElement {
	api?: UmbCollectionTextFilterApi;
	manifest?: ManifestCollectionTextFilter;
}
