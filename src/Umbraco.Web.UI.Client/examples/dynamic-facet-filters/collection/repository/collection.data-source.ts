import { filterProducts } from '../../data/products.js';
import type { ExampleProductFilterArgs } from '../../data/types.js';
import type { ExampleDynamicFacetCollectionFilterModel, ExampleProductCollectionItemModel } from './types.js';
import type { UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

const ENTITY_TYPE = 'example-product';

export class ExampleDynamicFacetCollectionDataSource
	implements UmbCollectionDataSource<ExampleProductCollectionItemModel, ExampleDynamicFacetCollectionFilterModel>
{
	// Alias-to-field mapping: this is the data source's responsibility
	#aliasFieldMap: Record<string, keyof ExampleProductFilterArgs> = {
		'Example.DynamicFacetFilter.CategoryFilter': 'categories',
		'Example.DynamicFacetFilter.SizeFilter': 'sizes',
		'Example.DynamicFacetFilter.ColorFilter': 'colors',
		'Example.DynamicFacetFilter.PriceFilter': 'priceRange',
	};

	// Reverse mapping: field -> alias
	#fieldAliasMap: Record<string, string> = Object.fromEntries(
		Object.entries(this.#aliasFieldMap).map(([alias, field]) => [field, alias]),
	);

	async getCollection(
		args: ExampleDynamicFacetCollectionFilterModel,
	): Promise<
		UmbDataSourceResponse<UmbPagedModel<ExampleProductCollectionItemModel> & { facets: Record<string, unknown> }>
	> {
		const productFilterArgs = this.#mapFiltersToArgs(args.filters ?? []);
		productFilterArgs.skip = args.skip;
		productFilterArgs.take = args.take;
		productFilterArgs.textFilter = args.filter;

		const result = filterProducts(productFilterArgs);

		// Map server models to client collection items
		const items: Array<ExampleProductCollectionItemModel> = result.items.map((product) => ({
			...product,
			unique: product.id,
			entityType: ENTITY_TYPE,
			icon: 'icon-shirt',
		}));

		// Map faceted results back using the alias mapping
		const facets: Record<string, unknown> = {};

		const toOptions = (facetItems: Array<{ unique: string; name: string; count: number }>) =>
			facetItems.map((f) => ({
				unique: f.unique,
				name: f.name,
				icon: '',
				entityType: '',
				count: f.count,
			}));

		facets[this.#fieldAliasMap['categories']] = toOptions(result.facets.categories);
		facets[this.#fieldAliasMap['sizes']] = toOptions(result.facets.sizes);
		facets[this.#fieldAliasMap['colors']] = toOptions(result.facets.colors);
		facets[this.#fieldAliasMap['priceRange']] = result.facets.priceRange;

		return {
			data: {
				items,
				total: result.total,
				facets,
			},
		};
	}

	#mapFiltersToArgs(filters: Array<UmbActiveFacetFilterModel>): ExampleProductFilterArgs {
		const args: ExampleProductFilterArgs = {};

		for (const filter of filters) {
			const field = this.#aliasFieldMap[filter.alias];
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
}
