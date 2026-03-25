export type * from './facet-filter.extension.js';
export type * from './facet-filter-api.interface.js';
export type * from './facet-filter-element.interface.js';
export type * from './select/types.js';

export interface UmbActiveFacetFilterModel extends UmbFacetFilterValueModel {}

export interface UmbFacetFilterValueModel {
	alias: string;
	unique: string;
	value: unknown;
}

export interface UmbWithOptionalFilteringModel {
	filters?: Array<UmbFacetFilterValueModel>;
}

export interface UmbWithFilteringModel {
	filters: Array<UmbFacetFilterValueModel>;
}
