import { UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_WEBHOOK_WORKSPACE_ALIAS } from '../../entity.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.WebhookDelivery.Collection',
		name: 'Webhook Delivery Workspace View',
		element: () => import('./webhook-delivery-collection-workspace-view.element.js'),
		meta: {
			label: 'Deliveries',
			pathname: 'deliveries',
			icon: 'icon-box-alt',
			collectionAlias: UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_WEBHOOK_WORKSPACE_ALIAS,
			},
		],
	},
];
