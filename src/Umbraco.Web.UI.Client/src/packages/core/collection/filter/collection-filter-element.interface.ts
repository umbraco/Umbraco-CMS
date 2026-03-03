import type { UmbCollectionFilterApi } from './collection-filter-api.interface.js';
import type { ManifestCollectionFilter } from './collection-filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbCollectionFilterElement extends UmbControllerHostElement {
	api?: UmbCollectionFilterApi;
	manifest?: ManifestCollectionFilter;
}
