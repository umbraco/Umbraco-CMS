import { EXAMPLE_PRODUCT_SIZE_ITEM_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_PRODUCT_SIZE_ITEM_REPOSITORY_ALIAS,
		name: 'Product Size Item Repository',
		api: () => import('./product-size-item.repository.js'),
	},
];
