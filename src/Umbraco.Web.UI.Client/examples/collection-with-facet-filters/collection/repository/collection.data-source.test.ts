import { products } from '../../data/products.js';
import { ExampleProductCollectionDataSource } from './collection.data-source.js';
import { expect } from '@open-wc/testing';

describe('ExampleProductCollectionDataSource', () => {
	const dataSource = new ExampleProductCollectionDataSource();

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

	describe('category filter', () => {
		it('returns only products in the given categories', async () => {
			const { data } = await dataSource.getCollection({ categories: ['cat-tshirt'] });
			expect(data!.items.every((p) => p.category === 'cat-tshirt')).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('size filter', () => {
		it('returns only products in the given sizes', async () => {
			const { data } = await dataSource.getCollection({ sizes: ['size-xl'] });
			expect(data!.items.every((p) => p.sizes.includes('size-xl'))).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('color filter', () => {
		it('returns only products in the given colors', async () => {
			const { data } = await dataSource.getCollection({ colors: ['color-black'] });
			expect(data!.items.every((p) => p.colors.includes('color-black'))).to.equal(true);
			expect(data!.items.length).to.be.greaterThan(0);
		});
	});

	describe('price range filter', () => {
		it('returns only products within the price range', async () => {
			const { data } = await dataSource.getCollection({ priceRange: { min: 20, max: 50 } });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.price.amount >= 20 && p.price.amount <= 50)).to.equal(true);
		});

		it('returns items within the full price range', async () => {
			const { data } = await dataSource.getCollection({ priceRange: { min: 0, max: 1000 } });
			expect(data!.items.length).to.equal(products.length);
		});
	});

	describe('combined filters', () => {
		it('applies category and price range together', async () => {
			const { data } = await dataSource.getCollection({
				categories: ['cat-jacket'],
				priceRange: { min: 80, max: 100 },
			});
			expect(data!.items.length).to.be.greaterThan(0);
			expect(
				data!.items.every((p) => p.category === 'cat-jacket' && p.price.amount >= 80 && p.price.amount <= 100),
			).to.equal(true);
		});
	});

	describe('facets', () => {
		it('returns the facets of each field as facet models', async () => {
			const { data } = await dataSource.getCollection({});
			const tshirts = data!.facets.categories.find((facet) => facet.unique === 'cat-tshirt');
			expect(tshirts).to.deep.equal({
				entityType: 'example-product-category',
				unique: 'cat-tshirt',
				count: products.filter((p) => p.category === 'cat-tshirt').length,
			});
			expect(data!.facets.sizes.length).to.be.greaterThan(0);
			expect(data!.facets.colors.length).to.be.greaterThan(0);
			expect(data!.facets.priceRange.max).to.be.greaterThan(data!.facets.priceRange.min);
		});
	});

	describe('text filter', () => {
		it('passes text filter through', async () => {
			const { data } = await dataSource.getCollection({ filter: 'hoodie' });
			expect(data!.items.length).to.be.greaterThan(0);
			expect(data!.items.every((p) => p.name.toLowerCase().includes('hoodie'))).to.equal(true);
		});
	});
});
