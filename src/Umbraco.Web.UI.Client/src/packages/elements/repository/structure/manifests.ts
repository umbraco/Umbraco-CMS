import { UMB_ELEMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS,
		name: 'Element Type Structure Repository',
		api: () => import('./element-type-structure.repository.js'),
	},
];
