import { UMB_WEBHOOK_DELIVERY_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_DELIVERY_COLLECTION_REPOSITORY_ALIAS,
		name: 'Webhook Delivery Collection Repository',
		api: () => import('./webhook-delivery-collection.repository.js'),
	},
];
