import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'kitchenSink',
	label: 'Kitchen Sink',
	icon: 'icon-box',
	visible: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
