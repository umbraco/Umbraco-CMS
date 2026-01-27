import { UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_PUBLISHING_REPOSITORY_ALIAS,
		name: 'Element Publishing Repository',
		api: () => import('./element-publishing.repository.js'),
	},
];
