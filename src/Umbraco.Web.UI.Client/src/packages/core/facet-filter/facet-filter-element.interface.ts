import type { UmbFacetFilterApi } from './facet-filter-api.interface.js';
import type { ManifestFacetFilter } from './facet-filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFacetFilterElement extends UmbControllerHostElement {
	api?: UmbFacetFilterApi;
	manifest?: ManifestFacetFilter;
}
