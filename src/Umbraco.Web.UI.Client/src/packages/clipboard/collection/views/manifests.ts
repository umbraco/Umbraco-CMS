import { UMB_CLIPBOARD_COLLECTION_ALIAS } from '../constants.js';
import { UMB_CLIPBOARD_TABLE_COLLECTION_VIEW_ALIAS } from './table/index.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_CLIPBOARD_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Clipboard Table Collection View',
		js: () => import('./table/clipboard-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_CLIPBOARD_COLLECTION_ALIAS,
			},
		],
	},
];
