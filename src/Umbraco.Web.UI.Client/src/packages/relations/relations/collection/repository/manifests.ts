import { UMB_RELATION_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_COLLECTION_REPOSITORY_ALIAS,
		name: 'Relation Collection Repository',
		api: () => import('./relation-collection.repository.js'),
	},
];
