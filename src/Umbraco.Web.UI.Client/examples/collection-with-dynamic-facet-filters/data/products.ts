import type { ExampleFilteredResult, ExampleProductFilterArgs, ExampleProductModel } from './types.js';

export const products: Array<ExampleProductModel> = [
	{
		id: 'p01',
		name: 'Classic White Tee',
		category: 'cat-tshirt',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-white'],
		price: { amount: 25, currency: 'USD' },
	},
	{
		id: 'p02',
		name: 'Graphic Print Tee',
		category: 'cat-tshirt',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-black', 'color-red'],
		price: { amount: 30, currency: 'USD' },
	},
	{
		id: 'p03',
		name: 'Slim Fit Trousers',
		category: 'cat-trousers',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-black', 'color-blue'],
		price: { amount: 55, currency: 'USD' },
	},
	{
		id: 'p04',
		name: 'Cargo Trousers',
		category: 'cat-trousers',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-green', 'color-black'],
		price: { amount: 65, currency: 'USD' },
	},
	{
		id: 'p05',
		name: 'Leather Jacket',
		category: 'cat-jacket',
		sizes: ['size-m', 'size-l'],
		colors: ['color-black'],
		price: { amount: 120, currency: 'USD' },
	},
	{
		id: 'p06',
		name: 'Denim Jacket',
		category: 'cat-jacket',
		sizes: ['size-s', 'size-m', 'size-l', 'size-xl'],
		colors: ['color-blue'],
		price: { amount: 85, currency: 'USD' },
	},
	{
		id: 'p07',
		name: 'Pullover Hoodie',
		category: 'cat-hoodie',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-black', 'color-white'],
		price: { amount: 50, currency: 'USD' },
	},
	{
		id: 'p08',
		name: 'Zip-Up Hoodie',
		category: 'cat-hoodie',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-red', 'color-blue'],
		price: { amount: 55, currency: 'USD' },
	},
	{
		id: 'p09',
		name: 'Running Shorts',
		category: 'cat-shorts',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-black', 'color-green'],
		price: { amount: 30, currency: 'USD' },
	},
	{
		id: 'p10',
		name: 'Chino Shorts',
		category: 'cat-shorts',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-white', 'color-blue'],
		price: { amount: 40, currency: 'USD' },
	},
	{
		id: 'p11',
		name: 'V-Neck Tee',
		category: 'cat-tshirt',
		sizes: ['size-s', 'size-m'],
		colors: ['color-white', 'color-blue'],
		price: { amount: 22, currency: 'USD' },
	},
	{
		id: 'p12',
		name: 'Oversized Tee',
		category: 'cat-tshirt',
		sizes: ['size-l', 'size-xl'],
		colors: ['color-black', 'color-green'],
		price: { amount: 28, currency: 'USD' },
	},
	{
		id: 'p13',
		name: 'Wide Leg Trousers',
		category: 'cat-trousers',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-white', 'color-black'],
		price: { amount: 60, currency: 'USD' },
	},
	{
		id: 'p14',
		name: 'Bomber Jacket',
		category: 'cat-jacket',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-black', 'color-green'],
		price: { amount: 95, currency: 'USD' },
	},
	{
		id: 'p15',
		name: 'Cropped Hoodie',
		category: 'cat-hoodie',
		sizes: ['size-s', 'size-m'],
		colors: ['color-white', 'color-red'],
		price: { amount: 45, currency: 'USD' },
	},
	{
		id: 'p16',
		name: 'Board Shorts',
		category: 'cat-shorts',
		sizes: ['size-m', 'size-l', 'size-xl'],
		colors: ['color-blue', 'color-red'],
		price: { amount: 35, currency: 'USD' },
	},
	{
		id: 'p17',
		name: 'Pocket Tee',
		category: 'cat-tshirt',
		sizes: ['size-s', 'size-m', 'size-l', 'size-xl'],
		colors: ['color-white', 'color-black', 'color-blue'],
		price: { amount: 20, currency: 'USD' },
	},
	{
		id: 'p18',
		name: 'Jogger Trousers',
		category: 'cat-trousers',
		sizes: ['size-s', 'size-m', 'size-l', 'size-xl'],
		colors: ['color-black', 'color-blue'],
		price: { amount: 50, currency: 'USD' },
	},
	{
		id: 'p19',
		name: 'Windbreaker Jacket',
		category: 'cat-jacket',
		sizes: ['size-s', 'size-m', 'size-l'],
		colors: ['color-red', 'color-white'],
		price: { amount: 75, currency: 'USD' },
	},
	{
		id: 'p20',
		name: 'Fleece Hoodie',
		category: 'cat-hoodie',
		sizes: ['size-l', 'size-xl'],
		colors: ['color-black', 'color-blue'],
		price: { amount: 60, currency: 'USD' },
	},
];

/**
 * Checks whether a product matches the given filter args, optionally excluding one dimension.
 * @param {ExampleProductModel} product - The product to test.
 * @param {ExampleProductFilterArgs} args - The active filter arguments.
 * @param {keyof ExampleProductFilterArgs} [excludeField] - A filter dimension to skip (for cross-facet counting).
 * @returns {boolean} True if the product matches all (non-excluded) filters.
 */
function matchesFilter(
	product: ExampleProductModel,
	args: ExampleProductFilterArgs,
	excludeField?: keyof ExampleProductFilterArgs,
): boolean {
	if (excludeField !== 'categories' && args.categories?.length) {
		if (!args.categories.includes(product.category)) return false;
	}
	if (excludeField !== 'sizes' && args.sizes?.length) {
		if (!args.sizes.some((v) => product.sizes.includes(v))) return false;
	}
	if (excludeField !== 'colors' && args.colors?.length) {
		if (!args.colors.some((v) => product.colors.includes(v))) return false;
	}
	if (excludeField !== 'priceRange' && args.priceRange) {
		if (product.price.amount < args.priceRange.min || product.price.amount > args.priceRange.max) return false;
	}
	return true;
}

/**
 * Counts unique values for a facet dimension, excluding its own filter to produce cross-facet counts.
 * @param {Array<ExampleProductModel>} items - The pre-filtered product list.
 * @param {ExampleProductFilterArgs} args - The active filter arguments.
 * @param {keyof ExampleProductFilterArgs} excludeField - The dimension to exclude from filtering.
 * @param {(product: ExampleProductModel) => Array<string>} accessor - Extracts the facet values from a product.
 * @returns {Array<{unique: string, count: number}>} Sorted facet counts.
 */
function countBy(
	items: Array<ExampleProductModel>,
	args: ExampleProductFilterArgs,
	excludeField: keyof ExampleProductFilterArgs,
	accessor: (product: ExampleProductModel) => Array<string>,
): Array<{ unique: string; count: number }> {
	const counts = new Map<string, number>();

	for (const product of products) {
		if (!matchesFilter(product, args, excludeField)) continue;
		for (const value of accessor(product)) {
			counts.set(value, (counts.get(value) ?? 0) + 1);
		}
	}

	return Array.from(counts.entries())
		.map(([unique, count]) => ({ unique, count }))
		.sort((a, b) => a.unique.localeCompare(b.unique));
}

/**
 * Filters, paginates and computes cross-facet counts for the product dataset.
 * @param {ExampleProductFilterArgs} args - Filter, pagination and text-search arguments.
 * @returns {ExampleFilteredResult} The filtered items, total count and faceted result data.
 */
export function filterProducts(args: ExampleProductFilterArgs): ExampleFilteredResult {
	let items = products.filter((p) => matchesFilter(p, args));

	if (args.textFilter) {
		const lower = args.textFilter.toLowerCase();
		items = items.filter((p) => p.name.toLowerCase().includes(lower));
	}

	// Cross-facet counts: for each dimension, apply all OTHER filters but not the current one
	const categories = countBy(items, args, 'categories', (p) => [p.category]);
	const sizes = countBy(items, args, 'sizes', (p) => p.sizes);
	const colors = countBy(items, args, 'colors', (p) => p.colors);

	// Price range from filtered items
	const prices = items.map((p) => p.price.amount);
	const priceRange = prices.length > 0 ? { min: Math.min(...prices), max: Math.max(...prices) } : { min: 0, max: 0 };

	const total = items.length;

	// Pagination
	const skip = args.skip || 0;
	const take = args.take || 50;
	items = items.slice(skip, skip + take);

	return {
		items,
		total,
		facets: { categories, sizes, colors, priceRange },
	};
}
