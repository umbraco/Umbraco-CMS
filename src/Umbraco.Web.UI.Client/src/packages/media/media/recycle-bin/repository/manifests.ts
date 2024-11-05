import { UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
		name: 'Media Recycle Bin Repository',
		api: () => import('./media-recycle-bin.repository.js'),
	},
];
