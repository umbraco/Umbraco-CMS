import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../entity.js';
import { UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS } from '../item/index.js';
import { UMB_CLIPBOARD_ENTRY_DETAIL_REPOSITORY_ALIAS, UMB_CLIPBOARD_ENTRY_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CLIPBOARD_ENTRY_DETAIL_REPOSITORY_ALIAS,
		name: 'Clipboard Detail Repository',
		api: () => import('./clipboard-entry-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_CLIPBOARD_ENTRY_DETAIL_STORE_ALIAS,
		name: 'Clipboard Detail Store',
		api: () => import('./clipboard-entry-detail.store.js'),
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.ClipboardEntry.Delete',
		name: 'Delete Dictionary Entry Entity Action',
		forEntityTypes: [UMB_CLIPBOARD_ENTRY_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_CLIPBOARD_ENTRY_DETAIL_REPOSITORY_ALIAS,
		},
	},
];
