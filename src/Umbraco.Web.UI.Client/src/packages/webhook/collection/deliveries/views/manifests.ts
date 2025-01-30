import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const UMB_WEBHOOK_DELIVERIES_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.WebhookDeliveries.Table';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_WEBHOOK_DELIVERIES_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Webhook Deliveries Table Collection View',
		js: () => import('./table/webhook-delivery-table-collection-view.element'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: "Umb.Collection.Webhook.Delivery",
			},
		],
	},
];
