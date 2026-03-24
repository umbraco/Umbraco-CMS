import type { ExampleFilteredResult, ExampleProductFilterArgs, ExampleProductModel } from './types.js';

export const products: Array<ExampleProductModel> = [
	{ id: 'p01', name: 'Classic White Tee', category: 'T-Shirt', sizes: ['S', 'M', 'L'], colors: ['White'], price: 25 },
	{
		id: 'p02',
		name: 'Graphic Print Tee',
		category: 'T-Shirt',
		sizes: ['M', 'L', 'XL'],
		colors: ['Black', 'Red'],
		price: 30,
	},
	{
		id: 'p03',
		name: 'Slim Fit Trousers',
		category: 'Trousers',
		sizes: ['S', 'M', 'L'],
		colors: ['Black', 'Blue'],
		price: 55,
	},
	{
		id: 'p04',
		name: 'Cargo Trousers',
		category: 'Trousers',
		sizes: ['M', 'L', 'XL'],
		colors: ['Green', 'Black'],
		price: 65,
	},
	{ id: 'p05', name: 'Leather Jacket', category: 'Jacket', sizes: ['M', 'L'], colors: ['Black'], price: 120 },
	{ id: 'p06', name: 'Denim Jacket', category: 'Jacket', sizes: ['S', 'M', 'L', 'XL'], colors: ['Blue'], price: 85 },
	{
		id: 'p07',
		name: 'Pullover Hoodie',
		category: 'Hoodie',
		sizes: ['M', 'L', 'XL'],
		colors: ['Black', 'White'],
		price: 50,
	},
	{ id: 'p08', name: 'Zip-Up Hoodie', category: 'Hoodie', sizes: ['S', 'M', 'L'], colors: ['Red', 'Blue'], price: 55 },
	{
		id: 'p09',
		name: 'Running Shorts',
		category: 'Shorts',
		sizes: ['S', 'M', 'L'],
		colors: ['Black', 'Green'],
		price: 30,
	},
	{
		id: 'p10',
		name: 'Chino Shorts',
		category: 'Shorts',
		sizes: ['M', 'L', 'XL'],
		colors: ['White', 'Blue'],
		price: 40,
	},
	{ id: 'p11', name: 'V-Neck Tee', category: 'T-Shirt', sizes: ['S', 'M'], colors: ['White', 'Blue'], price: 22 },
	{ id: 'p12', name: 'Oversized Tee', category: 'T-Shirt', sizes: ['L', 'XL'], colors: ['Black', 'Green'], price: 28 },
	{
		id: 'p13',
		name: 'Wide Leg Trousers',
		category: 'Trousers',
		sizes: ['S', 'M', 'L'],
		colors: ['White', 'Black'],
		price: 60,
	},
	{
		id: 'p14',
		name: 'Bomber Jacket',
		category: 'Jacket',
		sizes: ['M', 'L', 'XL'],
		colors: ['Black', 'Green'],
		price: 95,
	},
	{ id: 'p15', name: 'Cropped Hoodie', category: 'Hoodie', sizes: ['S', 'M'], colors: ['White', 'Red'], price: 45 },
	{ id: 'p16', name: 'Board Shorts', category: 'Shorts', sizes: ['M', 'L', 'XL'], colors: ['Blue', 'Red'], price: 35 },
	{
		id: 'p17',
		name: 'Pocket Tee',
		category: 'T-Shirt',
		sizes: ['S', 'M', 'L', 'XL'],
		colors: ['White', 'Black', 'Blue'],
		price: 20,
	},
	{
		id: 'p18',
		name: 'Jogger Trousers',
		category: 'Trousers',
		sizes: ['S', 'M', 'L', 'XL'],
		colors: ['Black', 'Blue'],
		price: 50,
	},
	{
		id: 'p19',
		name: 'Windbreaker Jacket',
		category: 'Jacket',
		sizes: ['S', 'M', 'L'],
		colors: ['Red', 'White'],
		price: 75,
	},
	{ id: 'p20', name: 'Fleece Hoodie', category: 'Hoodie', sizes: ['L', 'XL'], colors: ['Black', 'Blue'], price: 60 },
];

/**
 *
 * @param product
 * @param args
 * @param excludeField
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
		if (product.price < args.priceRange.min || product.price > args.priceRange.max) return false;
	}
	return true;
}

/**
 *
 * @param items
 * @param args
 * @param excludeField
 * @param accessor
 */
function countBy(
	items: Array<ExampleProductModel>,
	args: ExampleProductFilterArgs,
	excludeField: keyof ExampleProductFilterArgs,
	accessor: (product: ExampleProductModel) => Array<string>,
): Array<{ unique: string; name: string; count: number }> {
	const counts = new Map<string, number>();

	for (const product of products) {
		if (!matchesFilter(product, args, excludeField)) continue;
		for (const value of accessor(product)) {
			counts.set(value, (counts.get(value) ?? 0) + 1);
		}
	}

	return Array.from(counts.entries())
		.map(([value, count]) => ({ unique: value, name: value, count }))
		.sort((a, b) => a.name.localeCompare(b.name));
}

/**
 *
 * @param args
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
	const prices = items.map((p) => p.price);
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
