export const UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Webhook.Detail';
export const UMB_WEBHOOK_DETAIL_STORE_ALIAS = 'Umb.Store.Webhook.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS,
		name: 'Webhook Detail Repository',
		api: () => import('./webhook-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_WEBHOOK_DETAIL_STORE_ALIAS,
		name: 'Webhook Detail Store',
		api: () => import('./webhook-detail.store.js'),
	},
];
