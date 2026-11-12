import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type * from './active-value/types.js';
export type * from './filter.extension.js';
export type * from './filter-api.interface.js';
export type * from './filter-element.interface.js';
export type * from './select/types.js';
export type * from './value-resolver/filter-value-resolver.interface.js';

export interface UmbFilterValueModel {
	alias: string;
	value: unknown;
}

export interface UmbActiveFilterModel extends UmbFilterValueModel {
	unique: string;
}

/**
 * One value of a facet in a faceted result, for example a category and how many items in the result have it.
 */
export interface UmbFacetModel extends UmbEntityModel {
	unique: string;
	/**
	 * The number of items in the result that have this value.
	 */
	count?: number;
}
