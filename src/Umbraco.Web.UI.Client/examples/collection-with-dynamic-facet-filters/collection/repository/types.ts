import type { ExampleRangeFacetModel } from '../../range-kind/types.js';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbFacetModel } from '@umbraco-cms/backoffice/facet-filter';

export interface ExampleDynamicFacetCollectionFilterModel extends UmbCollectionFilterModel {
	categories?: Array<string>;
	sizes?: Array<string>;
	colors?: Array<string>;
	priceRange?: { min: number; max: number };
}

export interface ExampleProductCollectionItemModel extends UmbCollectionItemModel {
	name: string;
	category: string;
	sizes: Array<string>;
	colors: Array<string>;
	price: { amount: number; currency: string };
}

export interface ExampleProductCollectionFacetsModel {
	categories: Array<UmbFacetModel>;
	sizes: Array<UmbFacetModel>;
	colors: Array<UmbFacetModel>;
	priceRange: ExampleRangeFacetModel;
}
