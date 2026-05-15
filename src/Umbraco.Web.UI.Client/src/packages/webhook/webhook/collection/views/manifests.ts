import { UMB_WEBHOOK_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_WEBHOOK_EVENT_ITEMS_VALUE_TYPE } from '../../../webhook-event/value-type/constants.js';
import { UMB_BOOLEAN_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_WEBHOOK_COLLECTION_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: UMB_WEBHOOK_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Webhook Table Collection View',
		meta: {
			columns: [
				{
					field: 'url',
					label: '#webhooks_url',
				},
				{
					field: 'events',
					label: '#webhooks_events',
					valueType: UMB_WEBHOOK_EVENT_ITEMS_VALUE_TYPE,
				},
				{
					field: 'enabled',
					label: '#webhooks_enabled',
					valueType: UMB_BOOLEAN_VALUE_TYPE,
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_WEBHOOK_COLLECTION_ALIAS,
			},
		],
	},
];
