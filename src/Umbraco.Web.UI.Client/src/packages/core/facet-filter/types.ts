export type * from './active-value/types.js';
export type * from './facet-filter.extension.js';
export type * from './facet-filter-api.interface.js';
export type * from './facet-filter-element.interface.js';
export type * from './select/types.js';
export type * from './value-resolver/facet-filter-value-resolver.interface.js';

export interface UmbFacetFilterValueModel {
	alias: string;
	value: unknown;
}

export interface UmbActiveFacetFilterModel extends UmbFacetFilterValueModel {
	unique: string;
}

export interface UmbWithOptionalFilteringModel {
	filters?: Array<UmbFacetFilterValueModel>;
}

export interface UmbWithFilteringModel {
	filters: Array<UmbFacetFilterValueModel>;
}
