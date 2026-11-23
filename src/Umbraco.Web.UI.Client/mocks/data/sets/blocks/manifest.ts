import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'blocks',
	label: 'Blocks',
	icon: 'icon-brick',
	visible: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
