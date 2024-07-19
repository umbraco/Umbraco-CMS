import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_EVENT_REPOSITORY_ALIAS = 'Umb.Repository.Webhook.Event';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_WEBHOOK_EVENT_REPOSITORY_ALIAS,
	name: 'Webhook Event Repository',
	api: () => import('./webhook-event.repository.js'),
};

export const UMB_WEBHOOK_EVENT_STORE_ALIAS = 'Umb.Store.Webhook.Event';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_WEBHOOK_EVENT_STORE_ALIAS,
	name: 'Webhook Event Store',
	api: () => import('./webhook-event.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
