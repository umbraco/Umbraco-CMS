import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS, UMB_ELEMENT_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbElementDetailStore } from './element-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
		name: 'Element Detail Repository',
		api: () => import('./element-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_ELEMENT_DETAIL_STORE_ALIAS,
		name: 'Element Detail Store',
		api: UmbElementDetailStore,
	},
];
