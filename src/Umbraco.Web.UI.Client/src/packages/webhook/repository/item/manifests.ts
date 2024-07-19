import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.WebhookItem';
export const UMB_WEBHOOK_STORE_ALIAS = 'Umb.Store.WebhookItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_WEBHOOK_ITEM_REPOSITORY_ALIAS,
	name: 'Webhook Item Repository',
	api: () => import('./webhook-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_WEBHOOK_STORE_ALIAS,
	name: 'Webhook Item Store',
	api: () => import('./webhook-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
