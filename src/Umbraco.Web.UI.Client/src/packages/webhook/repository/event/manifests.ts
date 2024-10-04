export const UMB_WEBHOOK_EVENT_REPOSITORY_ALIAS = 'Umb.Repository.Webhook.Event';
export const UMB_WEBHOOK_EVENT_STORE_ALIAS = 'Umb.Store.Webhook.Event';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_EVENT_REPOSITORY_ALIAS,
		name: 'Webhook Event Repository',
		api: () => import('./webhook-event.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_WEBHOOK_EVENT_STORE_ALIAS,
		name: 'Webhook Event Store',
		api: () => import('./webhook-event.store.js'),
	},
];
