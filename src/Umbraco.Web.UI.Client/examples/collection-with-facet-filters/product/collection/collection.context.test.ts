import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS } from './filter/product-category/constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS } from './filter/product-color/constants.js';
import { EXAMPLE_PRODUCT_PRICE_FILTER_ALIAS } from './filter/price/constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS } from './filter/product-size/constants.js';
import { ExampleProductCollectionContext } from './collection.context.js';
import type { ExampleProductCollectionFacetsModel, ExampleProductCollectionItemModel } from './repository/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFacetedPagedModel } from '@umbraco-cms/backoffice/repository';

@customElement('example-product-collection-context-test-host')
class ExampleTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class ExampleTestProductCollectionContext extends ExampleProductCollectionContext {
	getFilterArgs() {
		return this._getFilterArgs();
	}

	getFacetedResults(
		data: UmbFacetedPagedModel<ExampleProductCollectionItemModel, ExampleProductCollectionFacetsModel>,
	) {
		return this._getFacetedResults(data);
	}
}

describe('ExampleProductCollectionContext', () => {
	let context: ExampleTestProductCollectionContext;

	beforeEach(() => {
		context = new ExampleTestProductCollectionContext(new ExampleTestHostElement());
	});

	it('maps the active filter values to the fields of the repository filter', async () => {
		context.filtering.setFilterValues(EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS, [
			{ unique: 'cat-jacket', value: 'cat-jacket' },
		]);
		context.filtering.setFilterValues(EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS, [
			{ unique: 'size-s', value: 'size-s' },
			{ unique: 'size-m', value: 'size-m' },
		]);
		context.filtering.setFilterValues(EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS, [
			{ unique: 'color-red', value: 'color-red' },
		]);
		context.filtering.setFilterValues(EXAMPLE_PRODUCT_PRICE_FILTER_ALIAS, [
			{ unique: 'min', value: { key: 'min', amount: 20 } },
			{ unique: 'max', value: { key: 'max', amount: 80 } },
		]);

		expect(await context.getFilterArgs()).to.deep.equal({
			categories: ['cat-jacket'],
			sizes: ['size-s', 'size-m'],
			colors: ['color-red'],
			priceRange: { min: 20, max: 80 },
		});
	});

	it('leaves out the fields of filters without active values', async () => {
		expect(await context.getFilterArgs()).to.deep.equal({});
	});

	it('keeps the other end of the price range open when one limit is removed', async () => {
		context.filtering.setFilterValues(EXAMPLE_PRODUCT_PRICE_FILTER_ALIAS, [
			{ unique: 'min', value: { key: 'min', amount: 20 } },
		]);

		expect((await context.getFilterArgs()).priceRange).to.deep.equal({ min: 20, max: Number.MAX_SAFE_INTEGER });
	});

	it('maps the facets of the response to the filters', () => {
		const facets: ExampleProductCollectionFacetsModel = {
			categories: [{ entityType: 'example-product-category', unique: 'cat-jacket', count: 4 }],
			sizes: [{ entityType: 'example-product-size', unique: 'size-s', count: 2 }],
			colors: [{ entityType: 'example-product-color', unique: 'color-red', count: 1 }],
			priceRange: { min: 20, max: 120 },
		};

		expect(context.getFacetedResults({ items: [], total: 0, facets })).to.deep.equal([
			{ alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_CATEGORY_FILTER_ALIAS, result: facets.categories },
			{ alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_SIZE_FILTER_ALIAS, result: facets.sizes },
			{ alias: EXAMPLE_PRODUCT_COLLECTION_PRODUCT_COLOR_FILTER_ALIAS, result: facets.colors },
			{ alias: EXAMPLE_PRODUCT_PRICE_FILTER_ALIAS, result: facets.priceRange },
		]);
	});
});
