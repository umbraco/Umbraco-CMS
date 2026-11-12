import { EXAMPLE_PRODUCT_COLOR_ITEM_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_PRODUCT_COLOR_ITEM_REPOSITORY_ALIAS,
		name: 'Product Color Item Repository',
		api: () => import('./product-color-item.repository.js'),
	},
];
