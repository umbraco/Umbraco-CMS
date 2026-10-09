import { UMB_CULTURE_REPOSITORY_ALIAS } from './constants.js';
import { UmbCultureRepository } from './culture.repository.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CULTURE_REPOSITORY_ALIAS,
		name: 'Cultures Repository',
		api: UmbCultureRepository,
	},
];
