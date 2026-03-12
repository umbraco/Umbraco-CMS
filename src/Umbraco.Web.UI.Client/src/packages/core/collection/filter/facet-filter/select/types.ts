import type { MetaCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface MetaCollectionFacetFilterSelect extends MetaCollectionFacetFilter {
	datalistDataSource: new (host: UmbControllerHost) => UmbDatalistDataSource;
	multiple?: boolean;
}

export interface UmbSelectOption {
	label: string;
	value: string;
}
