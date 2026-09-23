import { UMB_ELEMENT_TRANSFER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_TRANSFER_REPOSITORY_ALIAS,
		name: 'Element Transfer Repository',
		api: () => import('./element-transfer.repository.js'),
	},
];
