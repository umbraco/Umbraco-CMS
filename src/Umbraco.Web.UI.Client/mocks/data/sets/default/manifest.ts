import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'default',
	label: 'Default',
	icon: 'icon-home',
	visible: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
