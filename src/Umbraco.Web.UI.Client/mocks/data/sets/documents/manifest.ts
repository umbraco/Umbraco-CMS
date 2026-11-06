import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'documents',
	label: 'Documents',
	icon: 'icon-document',
	visible: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
