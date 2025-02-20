import { UMB_CLIPBOARD_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CLIPBOARD_COLLECTION_REPOSITORY_ALIAS,
		name: 'Clipboard Collection Repository',
		api: () => import('./clipboard-collection.repository.js'),
	},
];
