import type { ExampleColorFacetCount } from '../filters/color/types.js';

export interface ExampleProductModel {
	id: string;
	name: string;
	category: string;
	sizes: Array<string>;
	colors: Array<string>;
	price: { amount: number; currency: string };
}

export interface ExampleProductFilterArgs {
	skip?: number;
	take?: number;
	categories?: Array<string>;
	sizes?: Array<string>;
	colors?: Array<string>;
	priceRange?: { min: number; max: number };
	textFilter?: string;
}

export interface ExampleAttributeModel {
	id: string;
	name: string;
}

export interface ExampleFacetCount {
	unique: string;
	name: string;
	count: number;
}

export interface ExampleFacetedResultData {
	categories: Array<ExampleFacetCount>;
	sizes: Array<ExampleFacetCount>;
	colors: Array<ExampleColorFacetCount>;
	priceRange: { min: number; max: number };
}

export interface ExampleFilteredResult {
	items: Array<ExampleProductModel>;
	total: number;
	facets: ExampleFacetedResultData;
}
