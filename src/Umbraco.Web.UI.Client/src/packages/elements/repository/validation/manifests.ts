import { UMB_ELEMENT_VALIDATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_VALIDATION_REPOSITORY_ALIAS,
		name: 'Element Validation Repository',
		api: () => import('./element-validation.repository.js'),
	},
];
