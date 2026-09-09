import { UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS, UMB_RELATION_TYPE_ITEM_STORE_ALIAS } from './constants.js';
import { UmbRelationTypeItemStore } from './relation-type-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Relation Type Item Repository',
		api: () => import('./relation-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_RELATION_TYPE_ITEM_STORE_ALIAS,
		name: 'Relation Type Item Store',
		api: UmbRelationTypeItemStore,
	},
];
