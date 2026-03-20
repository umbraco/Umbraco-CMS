import type { MetaFacetFilter } from '../facet-filter.extension.js';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface MetaFacetFilterSelect extends MetaFacetFilter {
	datalistDataSource: new (host: UmbControllerHost) => UmbDatalistDataSource;
	multiple?: boolean;
}
