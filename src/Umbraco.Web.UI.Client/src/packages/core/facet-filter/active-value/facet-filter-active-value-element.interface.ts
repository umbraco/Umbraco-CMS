import type { UmbFacetFilterActiveValueApi } from './facet-filter-active-value-api.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFacetFilterActiveValueElement<
	TValue = unknown,
	TResolvedValue = unknown,
> extends UmbControllerHostElement {
	api?: UmbFacetFilterActiveValueApi<TValue, TResolvedValue>;
}
