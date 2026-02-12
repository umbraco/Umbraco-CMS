import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UMB_LANGUAGE_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbLanguageDetailStore } from './language-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		name: 'Language Detail Repository',
		api: () => import('./language-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_LANGUAGE_DETAIL_STORE_ALIAS,
		name: 'Language Detail Store',
		api: UmbLanguageDetailStore,
	},
];
