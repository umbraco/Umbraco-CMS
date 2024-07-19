import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Webhook.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_WEBHOOK_DETAIL_REPOSITORY_ALIAS,
	name: 'Webhook Detail Repository',
	api: () => import('./webhook-detail.repository.js'),
};

export const UMB_WEBHOOK_DETAIL_STORE_ALIAS = 'Umb.Store.Webhook.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_WEBHOOK_DETAIL_STORE_ALIAS,
	name: 'Webhook Detail Store',
	api: () => import('./webhook-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
