import { UMB_LANGUAGE_COLLECTION_ALIAS } from '../constants.js';
import { UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_BOOLEAN_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: UMB_LANGUAGE_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Language Table Collection View',
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
			columns: [
				{
					field: 'unique',
					label: 'ISO Code',
				},
				{
					field: 'isDefault',
					label: 'Default',
					valueType: UMB_BOOLEAN_VALUE_TYPE,
				},
				{
					field: 'isMandatory',
					label: 'Mandatory',
					valueType: UMB_BOOLEAN_VALUE_TYPE,
				},
				{
					field: 'fallbackIsoCode',
					label: 'Fallback',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_LANGUAGE_COLLECTION_ALIAS,
			},
		],
	},
];
