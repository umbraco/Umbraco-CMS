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

	describe('category filter mapping', () => {
		it('maps category filter alias to category field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.CategoryFilter', unique: 'T-Shirt', value: { unique: 'T-Shirt' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.category === 'T-Shirt')).to.be.true;
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('size filter mapping', () => {
		it('maps size filter alias to sizes field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.SizeFilter', unique: 'XL', value: { unique: 'XL' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.sizes.includes('XL'))).to.be.true;
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('color filter mapping', () => {
		it('maps color filter alias to colors field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.ColorFilter', unique: 'Black', value: { unique: 'Black' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.every((p) => p.colors.includes('Black'))).to.be.true;
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('price range filter mapping', () => {
		it('maps price range filter entries to priceRange field', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '20', value: { unique: '20' } },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '50', value: { unique: '50' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.price >= 20 && p.price <= 50)).to.be.true;
		});

		it('returns items within the full price range', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '0', value: { unique: '0' } },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '1000', value: { unique: '1000' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.equal(products.length);
		});
	});

	describe('combined filters', () => {
		it('applies category and price range together', async () => {
			const filters: Array<UmbActiveFacetFilterModel> = [
				{ alias: 'Example.DynamicFacetFilter.CategoryFilter', unique: 'Jacket', value: { unique: 'Jacket' } },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '80', value: { unique: '80' } },
				{ alias: 'Example.DynamicFacetFilter.PriceFilter', unique: '100', value: { unique: '100' } },
			];
			const { data } = await dataSource.getCollection({ filters });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.category === 'Jacket' && p.price >= 80 && p.price <= 100)).to.be.true;
		});
	});

	describe('faceted results', () => {
		it('returns faceted results keyed by alias', async () => {
			const { data } = await dataSource.getCollection({});
			const facets = (data as any).facets as Record<string, unknown>;
			expect(facets['Example.DynamicFacetFilter.CategoryFilter']).to.be.an('array');
			expect(facets['Example.DynamicFacetFilter.SizeFilter']).to.be.an('array');
			expect(facets['Example.DynamicFacetFilter.ColorFilter']).to.be.an('array');
			expect(facets['Example.DynamicFacetFilter.PriceFilter']).to.have.property('min');
			expect(facets['Example.DynamicFacetFilter.PriceFilter']).to.have.property('max');
		});
	});

	describe('text filter', () => {
		it('passes text filter through', async () => {
			const { data } = await dataSource.getCollection({ filter: 'hoodie' });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.name.toLowerCase().includes('hoodie'))).to.be.true;
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
