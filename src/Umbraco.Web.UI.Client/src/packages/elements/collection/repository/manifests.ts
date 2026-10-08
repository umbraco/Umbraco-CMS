import { UMB_ELEMENT_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_COLLECTION_REPOSITORY_ALIAS,
		name: 'Element Collection Repository',
		api: () => import('./element-collection.repository.js'),
	},
];
