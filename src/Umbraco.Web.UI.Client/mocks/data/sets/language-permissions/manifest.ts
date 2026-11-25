import type { UmbMockDataSet, UmbMockSetManifest } from '../../mock-data-set.types.js';

export const manifest: UmbMockSetManifest = {
	alias: 'languagePermissions',
	label: 'Language Permissions',
	icon: 'icon-globe',
	visible: true,
	userSwitcher: true,
	loader: () => import('./index.js') as Promise<UmbMockDataSet>,
};
