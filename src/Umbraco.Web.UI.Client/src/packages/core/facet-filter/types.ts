export type * from './facet-filter.extension.js';
export type * from './facet-filter-api.interface.js';
export type * from './facet-filter-element.interface.js';
export type * from './select/types.js';

export interface UmbActiveFacetFilterModel {
	alias: string;
	unique: string;
	value: any;
}

export interface UmbWithOptionalFilteringModel {
	filters?: Array<UmbActiveFacetFilterModel>;
}

export interface UmbWithFilteringModel {
	filters: Array<UmbActiveFacetFilterModel>;
}
