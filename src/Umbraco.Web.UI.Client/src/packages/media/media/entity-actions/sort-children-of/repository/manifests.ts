import { UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS,
		name: 'Sort Children Of Media Repository',
		api: () => import('./sort-children-of.repository.js'),
	},
];
