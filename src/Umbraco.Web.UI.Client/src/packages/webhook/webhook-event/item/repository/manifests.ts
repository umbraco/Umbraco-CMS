import { UmbWebhookEventItemStore } from './webhook-event-item.store.js';
import { UMB_WEBHOOK_EVENT_ITEM_REPOSITORY_ALIAS, UMB_WEBHOOK_EVENT_ITEM_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_EVENT_ITEM_REPOSITORY_ALIAS,
		name: 'Webhook Event Item Repository',
		api: () => import('./webhook-event-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_WEBHOOK_EVENT_ITEM_STORE_ALIAS,
		name: 'Webhook Event Item Store',
		api: UmbWebhookEventItemStore,
	},
];
