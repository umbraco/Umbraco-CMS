import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'multiBrandClothingShop',
	label: 'Multi Brand Clothing Shop',
	icon: 'icon-shopping-basket-alt-2',
	visible: true,
	examples: ['multi-brand-block-views'],
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
