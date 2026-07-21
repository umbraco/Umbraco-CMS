import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_DATE_TIME_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';
import { UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE } from '../../status-code/value-type/constants.js';
import { UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'Umb.CollectionView.WebhookDeliveries.Table',
		name: 'Webhook Deliveries Table Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS,
			},
		],
		meta: {
			columns: [
				{ field: 'date', label: '#general_date', valueType: UMB_DATE_TIME_VALUE_TYPE },
				{ field: 'url', label: '#webhooks_url' },
				{ field: 'statusCode', label: '#webhooks_statusCode', valueType: UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE },
				{ field: 'retryCount', label: '#webhooks_retryCount' },
			],
		},
	},
];
