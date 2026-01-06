import { UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		name: 'Element Recycle Bin Repository',
		api: () => import('./element-recycle-bin.repository.js'),
	},
];
