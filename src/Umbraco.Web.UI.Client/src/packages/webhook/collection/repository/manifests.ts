import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.WebhookCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
	name: 'Webhook Collection Repository',
	api: () => import('./webhook-collection.repository.js'),
};

export const manifests = [repository];
