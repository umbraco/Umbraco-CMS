import type { MetaFilter } from '@umbraco-cms/backoffice/filter';

export type ExampleMetaFilterRange = MetaFilter;

export interface ExampleRangeValueModel {
	key: string;
	amount: number;
}

export interface ExampleRangeFacetModel {
	min: number;
	max: number;
}
