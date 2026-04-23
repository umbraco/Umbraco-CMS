import { filterProducts, products } from './products.js';
import { expect } from '@open-wc/testing';

describe('filterProducts', () => {
	describe('no filters', () => {
		it('returns all products when no filters are applied', () => {
			const result = filterProducts({});
			expect(result.items.length).to.equal(products.length);
			expect(result.total).to.equal(products.length);
		});
	});

	describe('category filter', () => {
		it('filters by a single category', () => {
			const result = filterProducts({ categories: ['cat-tshirt'] });
			expect(result.items.every((p) => p.category === 'cat-tshirt')).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('filters by multiple categories', () => {
			const result = filterProducts({ categories: ['cat-tshirt', 'cat-hoodie'] });
			expect(result.items.every((p) => ['cat-tshirt', 'cat-hoodie'].includes(p.category))).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('returns no items for a non-existent category', () => {
			const result = filterProducts({ categories: ['NonExistent'] });
			expect(result.items.length).to.equal(0);
			expect(result.total).to.equal(0);
		});
	});

	describe('size filter', () => {
		it('filters by a single size', () => {
			const result = filterProducts({ sizes: ['size-xl'] });
			expect(result.items.every((p) => p.sizes.includes('size-xl'))).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('filters by multiple sizes (OR logic)', () => {
			const result = filterProducts({ sizes: ['size-s', 'size-xl'] });
			expect(result.items.every((p) => p.sizes.includes('size-s') || p.sizes.includes('size-xl'))).to.equal(true);
		});
	});

	describe('color filter', () => {
		it('filters by a single color', () => {
			const result = filterProducts({ colors: ['color-red'] });
			expect(result.items.every((p) => p.colors.includes('color-red'))).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('filters by multiple colors (OR logic)', () => {
			const result = filterProducts({ colors: ['color-red', 'color-green'] });
			expect(result.items.every((p) => p.colors.includes('color-red') || p.colors.includes('color-green'))).to.equal(true);
		});
	});

	describe('price range filter', () => {
		it('filters within a price range', () => {
			const result = filterProducts({ priceRange: { min: 20, max: 50 } });
			expect(result.items.length).to.be.greaterThan(0);
			expect(result.items.every((p) => p.price.amount >= 20 && p.price.amount <= 50)).to.equal(true);
		});

		it('includes items at the boundary values', () => {
			const result = filterProducts({ priceRange: { min: 25, max: 25 } });
			expect(result.items.length).to.be.greaterThan(0);
			expect(result.items.every((p) => p.price.amount === 25)).to.equal(true);
		});

		it('returns no items when range excludes all products', () => {
			const result = filterProducts({ priceRange: { min: 200, max: 300 } });
			expect(result.items.length).to.equal(0);
		});

		it('filters correctly with a wide range', () => {
			const result = filterProducts({ priceRange: { min: 0, max: 1000 } });
			expect(result.items.length).to.equal(products.length);
		});
	});

	describe('combined filters', () => {
		it('applies category and size filters together (AND logic)', () => {
			const result = filterProducts({ categories: ['cat-tshirt'], sizes: ['size-xl'] });
			expect(result.items.every((p) => p.category === 'cat-tshirt' && p.sizes.includes('size-xl'))).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('applies category and price range together', () => {
			const result = filterProducts({ categories: ['cat-jacket'], priceRange: { min: 80, max: 100 } });
			expect(result.items.every((p) => p.category === 'cat-jacket' && p.price.amount >= 80 && p.price.amount <= 100)).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('applies all filters together', () => {
			const result = filterProducts({
				categories: ['cat-tshirt'],
				sizes: ['size-m'],
				colors: ['color-black'],
				priceRange: { min: 20, max: 35 },
			});
			expect(
				result.items.every(
					(p) =>
						p.category === 'cat-tshirt' &&
						p.sizes.includes('size-m') &&
						p.colors.includes('color-black') &&
						p.price.amount >= 20 &&
						p.price.amount <= 35,
				),
			).to.equal(true);
		});
	});

	describe('text filter', () => {
		it('filters by text (case-insensitive)', () => {
			const result = filterProducts({ textFilter: 'hoodie' });
			expect(result.items.every((p) => p.name.toLowerCase().includes('hoodie'))).to.equal(true);
			expect(result.items.length).to.be.greaterThan(0);
		});

		it('combines text filter with category filter', () => {
			const result = filterProducts({ categories: ['cat-hoodie'], textFilter: 'zip' });
			expect(result.items.every((p) => p.category === 'cat-hoodie' && p.name.toLowerCase().includes('zip'))).to.equal(
				true,
			);
			expect(result.items.length).to.be.greaterThan(0);
		});
	});

	describe('pagination', () => {
		it('returns the first page of results', () => {
			const result = filterProducts({ take: 5 });
			expect(result.items.length).to.equal(5);
			expect(result.total).to.equal(products.length);
		});

		it('returns the second page of results', () => {
			const result = filterProducts({ skip: 5, take: 5 });
			expect(result.items.length).to.equal(5);
			expect(result.total).to.equal(products.length);
		});

		it('returns remaining items on the last page', () => {
			const result = filterProducts({ skip: 18, take: 5 });
			expect(result.items.length).to.equal(2);
			expect(result.total).to.equal(products.length);
		});
	});

	describe('faceted counts', () => {
		it('returns category facet counts for unfiltered results', () => {
			const result = filterProducts({});
			const tshirtFacet = result.facets.categories.find((f) => f.unique === 'cat-tshirt');
			expect(tshirtFacet).to.not.equal(undefined);
			expect(tshirtFacet!.count).to.equal(products.filter((p) => p.category === 'cat-tshirt').length);
		});

		it('returns size facet counts', () => {
			const result = filterProducts({});
			const xlFacet = result.facets.sizes.find((f) => f.unique === 'size-xl');
			expect(xlFacet).to.not.equal(undefined);
			expect(xlFacet!.count).to.be.greaterThan(0);
		});

		it('returns color facet counts', () => {
			const result = filterProducts({});
			const blackFacet = result.facets.colors.find((f) => f.unique === 'color-black');
			expect(blackFacet).to.not.equal(undefined);
			expect(blackFacet!.count).to.be.greaterThan(0);
		});

		it('returns price range from filtered items', () => {
			const result = filterProducts({ categories: ['cat-tshirt'] });
			const tshirtPrices = products.filter((p) => p.category === 'cat-tshirt').map((p) => p.price.amount);
			expect(result.facets.priceRange.min).to.equal(Math.min(...tshirtPrices));
			expect(result.facets.priceRange.max).to.equal(Math.max(...tshirtPrices));
		});

		it('category facet counts reflect cross-filtering (exclude own dimension)', () => {
			// When filtering by size S, category counts should reflect all products with size S
			const result = filterProducts({ sizes: ['size-s'] });
			const totalCategoryCount = result.facets.categories.reduce((sum, f) => sum + f.count, 0);
			// Category counts should be based on products matching size S (but not re-filtered by category)
			const productsWithSizeS = products.filter((p) => p.sizes.includes('size-s'));
			expect(totalCategoryCount).to.equal(productsWithSizeS.length);
		});
	});
});
