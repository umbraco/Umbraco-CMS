import type { UmbCollectionFacetFilterApi } from './collection-facet-filter-api.interface.js';
import type { ManifestCollectionFacetFilter } from './collection-facet-filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbCollectionFacetFilterElement extends UmbControllerHostElement {
	api?: UmbCollectionFacetFilterApi;
	manifest?: ManifestCollectionFacetFilter;
}
