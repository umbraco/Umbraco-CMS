import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS = 'Umb.Repository.Document.Notifications';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_NOTIFICATIONS_REPOSITORY_ALIAS,
	name: 'Document Notifications Repository',
	api: () => import('./document-notifications.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
