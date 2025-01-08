import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS, UMB_SCRIPT_DETAIL_STORE_ALIAS } from './constants.js';
import { manifests as itemManifests } from './item/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
		name: 'Script Detail Repository',
		api: () => import('./script-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_SCRIPT_DETAIL_STORE_ALIAS,
		name: 'Script Detail Store',
		api: () => import('./script-detail.store.js'),
	},
	...itemManifests,
];
