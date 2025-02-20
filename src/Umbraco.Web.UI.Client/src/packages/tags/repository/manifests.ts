import { UMB_TAG_REPOSITORY_ALIAS, UMB_TAG_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TAG_REPOSITORY_ALIAS,
		name: 'Tags Repository',
		api: () => import('./tag.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_TAG_STORE_ALIAS,
		name: 'Tags Store',
		api: () => import('./tag.store.js'),
	},
];
