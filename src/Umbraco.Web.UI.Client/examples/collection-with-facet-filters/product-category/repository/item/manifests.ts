import { EXAMPLE_PRODUCT_CATEGORY_ITEM_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_PRODUCT_CATEGORY_ITEM_REPOSITORY_ALIAS,
		name: 'Product Category Item Repository',
		api: () => import('./product-category-item.repository.js'),
	},
];
