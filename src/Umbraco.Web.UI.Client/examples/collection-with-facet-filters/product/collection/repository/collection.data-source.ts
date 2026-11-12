import { EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE } from '../../../product-category/entity.js';
import { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../../product-color/entity.js';
import { EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE } from '../../../product-size/entity.js';
import { filterProducts } from '../../../mock-data/products.js';
import type { ExampleMockFacetCount } from '../../../mock-data/products.js';
import type {
	ExampleProductCollectionFilterModel,
	ExampleProductCollectionFacetsModel,
	ExampleProductCollectionItemModel,
} from './types.js';
import type { UmbFacetModel } from '@umbraco-cms/backoffice/filter';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbDataSourceResponse, UmbFacetedPagedModel } from '@umbraco-cms/backoffice/repository';

export class ExampleProductCollectionDataSource implements UmbCollectionDataSource<
	ExampleProductCollectionItemModel,
	ExampleProductCollectionFilterModel
> {
	async getCollection(
		args: ExampleProductCollectionFilterModel,
	): Promise<
		UmbDataSourceResponse<UmbFacetedPagedModel<ExampleProductCollectionItemModel, ExampleProductCollectionFacetsModel>>
	> {
		const result = filterProducts({
			skip: args.skip,
			take: args.take,
			textFilter: args.filter,
			categories: args.categories,
			sizes: args.sizes,
			colors: args.colors,
			priceRange: args.priceRange,
		});

		const items: Array<ExampleProductCollectionItemModel> = result.items.map((product) => ({
			...product,
			unique: product.id,
			entityType: 'example-product',
			icon: 'icon-shirt',
		}));

		const toFacets = (counts: Array<ExampleMockFacetCount>, entityType: string): Array<UmbFacetModel> =>
			counts.map((count) => ({ entityType, unique: count.unique, count: count.count }));

		return {
			data: {
				items,
				total: result.total,
				facets: {
					categories: toFacets(result.facets.categories, EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE),
					sizes: toFacets(result.facets.sizes, EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE),
					colors: toFacets(result.facets.colors, EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE),
					priceRange: result.facets.priceRange,
				},
			},
		};
	}
}
