export const UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.WebhookCollection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
		name: 'Webhook Collection Repository',
		api: () => import('./webhook-collection.repository.js'),
	},
];
