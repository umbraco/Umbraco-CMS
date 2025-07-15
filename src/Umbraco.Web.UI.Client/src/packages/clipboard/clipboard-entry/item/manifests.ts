import { UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS, UMB_CLIPBOARD_ENTRY_ITEM_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS,
		name: 'Clipboard Entry Item Repository',
		api: () => import('./clipboard-entry-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_CLIPBOARD_ENTRY_ITEM_STORE_ALIAS,
		name: 'Clipboard Entry Item Store',
		api: () => import('./clipboard-entry-item.store.js'),
	},
];
