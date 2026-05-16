import { UMB_WEBHOOK_EVENT_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_EVENT_COLLECTION_REPOSITORY_ALIAS,
		name: 'Webhook Event Collection Repository',
		api: () => import('./webhook-event-collection.repository.js'),
	},
];
