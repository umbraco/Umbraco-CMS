import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'blocksReusableContent',
	label: 'Blocks: Reusable Content',
	icon: 'icon-layers',
	visible: true,
	userSwitcher: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
