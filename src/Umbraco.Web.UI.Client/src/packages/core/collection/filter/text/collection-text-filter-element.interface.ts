import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import type { ManifestCollectionTextFilter } from './collection-text-filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbCollectionTextFilterElement extends UmbControllerHostElement {
	api?: UmbCollectionTextFilterApi;
	manifest?: ManifestCollectionTextFilter;
}
