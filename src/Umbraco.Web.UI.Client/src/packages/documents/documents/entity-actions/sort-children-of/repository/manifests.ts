import { UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Sort Children Of Document Repository',
		api: () => import('./sort-children-of.repository.js'),
	},
];
