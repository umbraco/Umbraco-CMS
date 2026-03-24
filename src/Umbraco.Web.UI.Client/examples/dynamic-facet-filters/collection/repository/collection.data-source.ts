import { filterProducts } from '../../data/products.js';
import type { ExampleProductFilterArgs, ExampleProductModel } from '../../data/types.js';
import type { ExampleDynamicFacetCollectionFilterModel } from './types.js';
import type { UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

// Alias-to-field mapping: this is the data source's responsibility
const ALIAS_FIELD_MAP: Record<string, keyof ExampleProductFilterArgs> = {
	'Example.DynamicFacetFilter.CategoryFilter': 'categories',
	'Example.DynamicFacetFilter.SizeFilter': 'sizes',
	'Example.DynamicFacetFilter.ColorFilter': 'colors',
	'Example.DynamicFacetFilter.PriceFilter': 'priceRange',
};

/**
 *
 * @param filters
 */
function mapFiltersToArgs(filters: Array<UmbActiveFacetFilterModel>): ExampleProductFilterArgs {
	const args: ExampleProductFilterArgs = {};

	for (const filter of filters) {
		const field = ALIAS_FIELD_MAP[filter.alias];
		if (!field) continue;

		const filterValue = filter.value?.unique ?? filter.unique;

		switch (field) {
			case 'categories':
				args.categories ??= [];
				args.categories.push(filterValue);
				break;
			case 'sizes':
				args.sizes ??= [];
				args.sizes.push(filterValue);
				break;
			case 'colors':
				args.colors ??= [];
				args.colors.push(filterValue);
				break;
			case 'priceRange': {
				const value = parseFloat(filterValue);
				if (!args.priceRange) {
					args.priceRange = { min: value, max: value };
				} else {
					args.priceRange = {
						min: Math.min(args.priceRange.min, value),
						max: Math.max(args.priceRange.max, value),
					};
				}
				break;
			}
		}
	}

	return args;
}

// Reverse mapping: field -> alias
const FIELD_ALIAS_MAP: Record<string, string> = Object.fromEntries(
	Object.entries(ALIAS_FIELD_MAP).map(([alias, field]) => [field, alias]),
);

export interface ExampleDynamicFacetCollectionDataSourceResult {
	data: UmbPagedModel<ExampleProductModel>;
	facets: Record<string, unknown>;
}

export class ExampleDynamicFacetCollectionDataSource
	implements UmbCollectionDataSource<ExampleProductModel, ExampleDynamicFacetCollectionFilterModel>
{
	async getCollection(
		args: ExampleDynamicFacetCollectionFilterModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ExampleProductModel> & { facets: Record<string, unknown> }>> {
		// Map the generic filter array into data-source-specific args
		const productFilterArgs = mapFiltersToArgs(args.filters ?? []);
		productFilterArgs.skip = args.skip;
		productFilterArgs.take = args.take;
		productFilterArgs.textFilter = args.filter;

		const result = filterProducts(productFilterArgs);

		// Map faceted results back using the alias mapping
		const facets: Record<string, unknown> = {};

		const toOptions = (items: Array<{ unique: string; name: string; count: number }>) =>
			items.map((f) => ({
				unique: f.unique,
				name: f.name,
				icon: '',
				entityType: '',
				count: f.count,
			}));

		facets[FIELD_ALIAS_MAP['categories']] = toOptions(result.facets.categories);
		facets[FIELD_ALIAS_MAP['sizes']] = toOptions(result.facets.sizes);
		facets[FIELD_ALIAS_MAP['colors']] = toOptions(result.facets.colors);
		facets[FIELD_ALIAS_MAP['priceRange']] = result.facets.priceRange;

		return {
			data: {
				items: result.items,
				total: result.total,
				facets,
			},
		};
	}
}
