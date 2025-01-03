import { UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
		name: 'Webhook Collection Repository',
		api: () => import('./webhook-collection.repository.js'),
	},
];
