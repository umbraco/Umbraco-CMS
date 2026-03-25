import { products } from '../../data/products.js';
import { ExampleDynamicFacetCollectionDataSource } from './collection.data-source.js';
import { expect } from '@open-wc/testing';
import type { UmbActiveFacetFilterModel } from '@umbraco-cms/backoffice/facet-filter';

describe('ExampleDynamicFacetCollectionDataSource', () => {
	const dataSource = new ExampleDynamicFacetCollectionDataSource();

	describe('no filters', () => {
		it('returns all products when no filters are provided', async () => {
			const { data } = await dataSource.getCollection({});
			expect(data!.items.length).to.equal(products.length);
			expect(data!.total).to.equal(products.length);
		});
	});

	describe('model mapping', () => {
		it('maps id to unique and adds entityType and icon', async () => {
			const { data } = await dataSource.getCollection({});
			const first = data!.items[0];
			expect(first.unique).to.equal(products[0].id);
			expect(first.entityType).to.equal('example-product');
			expect(first.icon).to.equal('icon-shirt');
		});

		it('preserves product data fields', async () => {
			const { data } = await dataSource.getCollection({});
			const first = data!.items[0];
			expect(first.name).to.equal(products[0].name);
			expect(first.category).to.equal(products[0].category);
			expect(first.price).to.deep.equal(products[0].price);
		});
	});

	describe('category filter mapping', () => {
		it('maps category filter alias to category field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.CategoryFilter', unique: 'cat-tshirt', value: { unique: 'cat-tshirt' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.category === 'cat-tshirt')).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('size filter mapping', () => {
		it('maps size filter alias to sizes field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.SizeFilter', unique: 'size-xl', value: { unique: 'size-xl' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.sizes.includes('size-xl'))).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('color filter mapping', () => {
		it('maps color filter alias to colors field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.ColorFilter', unique: 'color-black', value: { unique: 'color-black' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.colors.includes('color-black'))).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('price range filter mapping', () => {
		it('maps price range filter entries to priceRange field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'min', value: 20 },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'max', value: 50 },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.price.amount >= 20 && p.price.amount <= 50)).to.equal(true);
		});

		it('returns items within the full price range', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'min', value: 0 },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'max', value: 1000 },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.equal(products.length);
		});
	});

	describe('combined filters', () => {
		it('applies category and price range together', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.CategoryFilter', unique: 'cat-jacket', value: { unique: 'cat-jacket' } },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'min', value: 80 },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: 'max', value: 100 },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.category === 'cat-jacket' && p.price.amount >= 80 && p.price.amount <= 100)).to.equal(true);
		});
	});

	describe('faceted results', () => {
		it('returns faceted results keyed by alias', async () => {
			const { data } = await dataSource.getCollection({});
			const facets = data!.facets;
			expect(Array.isArray(facets['Example.DynamicFacetFilter.CategoryFilter'])).to.equal(true);
			expect(Array.isArray(facets['Example.DynamicFacetFilter.SizeFilter'])).to.equal(true);
			expect(Array.isArray(facets['Example.DynamicFacetFilter.ColorFilter'])).to.equal(true);
			const priceRange = facets['Example.DynamicFacetFilter.PriceFilter'] as Record<string, unknown>;
			expect('min' in priceRange).to.equal(true);
			expect('max' in priceRange).to.equal(true);
		});
	});

	describe('text filter', () => {
		it('passes text filter through', async () => {
			const { data } = await dataSource.getCollection({ filter: 'hoodie' });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.name.toLowerCase().includes('hoodie'))).to.equal(true);
		});
	});

	describe('unknown alias', () => {
		it('ignores filters with unknown aliases', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Unknown.Filter', unique: 'value', value: { unique: 'value' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.equal(products.length);
		});
	});
});
