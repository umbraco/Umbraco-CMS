export const UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.WebhookItem';
export const UMB_WEBHOOK_STORE_ALIAS = 'Umb.Store.WebhookItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS,
		name: 'Webhook Item Repository',
		api: () => import('./webhook-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_WEBHOOK_STORE_ALIAS,
		name: 'Webhook Item Store',
		api: () => import('./webhook-item.store.js'),
	},
];
