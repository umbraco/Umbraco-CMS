import { UMB_WEBHOOK_DELIVERIES_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_DELIVERIES_COLLECTION_REPOSITORY_ALIAS,
		name: 'Webhook Deliveries Collection Repository',
		api: () => import('./webhook-delivery-collection.repository.js'),
	},
];
