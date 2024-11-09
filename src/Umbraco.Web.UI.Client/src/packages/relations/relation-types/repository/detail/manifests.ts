import { UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_RELATION_TYPE_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Relation Type Detail Repository',
		api: () => import('./relation-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_RELATION_TYPE_DETAIL_STORE_ALIAS,
		name: 'Relation Type Detail Store',
		api: () => import('./relation-type-detail.store.js'),
	},
];
