export interface ExampleProductModel {
	unique: string;
	entityType: string;
	name: string;
	category: string;
	sizes: Array<string>;
	colors: Array<string>;
	price: number;
	icon: string;
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

export interface ExampleFacetCount {
	unique: string;
	name: string;
	count: number;
}

export interface ExampleFacetedResultData {
	categories: Array<ExampleFacetCount>;
	sizes: Array<ExampleFacetCount>;
	colors: Array<ExampleFacetCount>;
	priceRange: { min: number; max: number };
}

export interface ExampleFilteredResult {
	items: Array<ExampleProductModel>;
	total: number;
	facets: ExampleFacetedResultData;
}
