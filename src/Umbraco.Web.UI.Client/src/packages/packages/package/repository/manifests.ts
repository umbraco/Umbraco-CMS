import { UMB_PACKAGE_REPOSITORY_ALIAS, UMB_PACKAGE_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PACKAGE_REPOSITORY_ALIAS,
		name: 'Package Repository',
		api: () => import('./package.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_PACKAGE_STORE_ALIAS,
		name: 'Package Store',
		api: () => import('./package.store.js'),
	},
];
