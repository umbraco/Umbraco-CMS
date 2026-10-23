import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'userPermissions',
	label: 'User Permissions',
	icon: 'icon-lock',
	visible: false,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
