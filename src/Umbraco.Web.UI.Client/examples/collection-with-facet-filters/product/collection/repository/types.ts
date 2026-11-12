import type { ExampleProductPriceModel } from '../../price/types.js';
import type { ExampleProductPriceFacetModel } from '../filter/product-price/types.js';
import type { UmbFacetModel } from '@umbraco-cms/backoffice/filter';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface ExampleProductCollectionFilterModel extends UmbCollectionFilterModel {
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
	price: ExampleProductPriceModel;
}

export interface ExampleProductCollectionFacetsModel {
	categories: Array<UmbFacetModel>;
	sizes: Array<UmbFacetModel>;
	colors: Array<UmbFacetModel>;
	priceRange: ExampleProductPriceFacetModel;
}
