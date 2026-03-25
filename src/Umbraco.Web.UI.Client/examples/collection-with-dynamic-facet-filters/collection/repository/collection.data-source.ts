import { filterProducts } from '../../data/products.js';
import type { ExampleProductFilterArgs } from '../../data/types.js';
import type { ExampleRangeValueModel } from '../../range-kind/types.js';
import type { ExampleDynamicFacetCollectionFilterModel, ExampleProductCollectionItemModel } from './types.js';
import type { UmbFacetFilterValueModel } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

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
			entityType: 'example-product',
			icon: 'icon-shirt',
		}));

		// Map faceted results back using the alias mapping
		const facets: Record<string, unknown> = {};

		const toOptions = (facetItems: Array<{ unique: string; count: number }>, entityType: string) =>
			facetItems.map((f) => ({
				unique: f.unique,
				entityType,
				count: f.count,
			}));

		facets[this.#fieldAliasMap['categories']] = toOptions(result.facets.categories, 'example-product-cateory');
		facets[this.#fieldAliasMap['sizes']] = toOptions(result.facets.sizes, 'example-product-size');
		facets[this.#fieldAliasMap['colors']] = toOptions(result.facets.colors, 'example-product-color');
		facets[this.#fieldAliasMap['priceRange']] = result.facets.priceRange;

		return {
			data: {
				items,
				total: result.total,
				facets,
			},
		};
	}

	#mapFiltersToArgs(filters: Array<UmbFacetFilterValueModel>): ExampleProductFilterArgs {
		const args: ExampleProductFilterArgs = {};

		for (const filter of filters) {
			const field = this.#aliasFieldMap[filter.alias];
			if (!field) continue;

			switch (field) {
				case 'categories':
					args.categories ??= [];
					args.categories.push(filter.value as string);
					break;
				case 'sizes':
					args.sizes ??= [];
					args.sizes.push(filter.value as string);
					break;
				case 'colors':
					args.colors ??= [];
					args.colors.push(filter.value as string);
					break;
				case 'priceRange': {
					args.priceRange ??= { min: 0, max: 0 };
					const rangeValue = filter.value as ExampleRangeValueModel;
					if (rangeValue.key === 'min') args.priceRange.min = rangeValue.amount;
					if (rangeValue.key === 'max') args.priceRange.max = rangeValue.amount;
					break;
				}
			}
		}

		return args;
	}
}
