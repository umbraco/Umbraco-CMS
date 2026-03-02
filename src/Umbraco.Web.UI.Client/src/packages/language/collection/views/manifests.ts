import { UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Language Table Collection View',
		js: () => import('./table/language-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Language',
			},
		],
	},
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'TEST',
		name: 'Language Table Collection View',
		meta: {
			label: 'Table KIND',
			icon: 'icon-table',
			pathName: 'table2',
			columns: [
				{
					field: 'unique',
					headerName: 'ISO Code',
				},
				{
					field: 'isDefault',
					headerName: 'Default',
				},
				{
					field: 'isMandatory',
					headerName: 'Mandatory',
				},
				{
					field: 'fallbackIsoCode',
					headerName: 'Fallback',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Language',
			},
		],
	},
];
