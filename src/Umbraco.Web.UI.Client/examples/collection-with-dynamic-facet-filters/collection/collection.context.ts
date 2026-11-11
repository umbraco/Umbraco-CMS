import { EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS } from '../filters/category/constants.js';
import { EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS } from '../filters/color/constants.js';
import { EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS } from '../filters/price/constants.js';
import { EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS } from '../filters/size/constants.js';
import type { ExampleRangeValueModel } from '../range-kind/types.js';
import { EXAMPLE_PRODUCT_TABLE_COLLECTION_VIEW_ALIAS } from '../table-view/constants.js';
import type {
	ExampleDynamicFacetCollectionFilterModel,
	ExampleProductCollectionFacetsModel,
	ExampleProductCollectionItemModel,
} from './repository/types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbFacetedPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFacetedResultModel } from '@umbraco-cms/backoffice/facet-filter';

/**
 * Maps the active facet filter values to the product repository's filter model, and the repository's facets back to
 * the facet filters, so the repository never deals with filter aliases.
 */
export class ExampleProductCollectionContext extends UmbDefaultCollectionContext<
	ExampleProductCollectionItemModel,
	ExampleDynamicFacetCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, EXAMPLE_PRODUCT_TABLE_COLLECTION_VIEW_ALIAS);
	}

	protected override async _getFilterArgs(): Promise<Partial<ExampleDynamicFacetCollectionFilterModel>> {
		const filterValues = await this.filtering.getActiveFilterValues();
		const valuesOf = (alias: string) => filterValues.filter((f) => f.alias === alias).map((f) => f.value);
		const args: Partial<ExampleDynamicFacetCollectionFilterModel> = {};

		const categories = valuesOf(EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS) as Array<string>;
		if (categories.length) args.categories = categories;

		const sizes = valuesOf(EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS) as Array<string>;
		if (sizes.length) args.sizes = sizes;

		const colors = valuesOf(EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS) as Array<string>;
		if (colors.length) args.colors = colors;

		const priceLimits = valuesOf(EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS) as Array<ExampleRangeValueModel>;
		if (priceLimits.length) {
			args.priceRange = {
				min: priceLimits.find((limit) => limit.key === 'min')?.amount ?? 0,
				max: priceLimits.find((limit) => limit.key === 'max')?.amount ?? Number.MAX_SAFE_INTEGER,
			};
		}

		return args;
	}

	protected override _getFacetedResults(
		data: UmbFacetedPagedModel<ExampleProductCollectionItemModel, ExampleProductCollectionFacetsModel>,
	): Array<UmbFacetedResultModel> {
		return [
			{ alias: EXAMPLE_PRODUCT_CATEGORY_FACET_FILTER_ALIAS, result: data.facets.categories },
			{ alias: EXAMPLE_PRODUCT_SIZE_FACET_FILTER_ALIAS, result: data.facets.sizes },
			{ alias: EXAMPLE_PRODUCT_COLOR_FACET_FILTER_ALIAS, result: data.facets.colors },
			{ alias: EXAMPLE_PRODUCT_PRICE_FACET_FILTER_ALIAS, result: data.facets.priceRange },
		];
	}
}

export { ExampleProductCollectionContext as api };
