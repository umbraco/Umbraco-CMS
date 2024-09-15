import { UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Relation Type Collection Repository',
		api: () => import('./relation-type-collection.repository.js'),
	},
];
