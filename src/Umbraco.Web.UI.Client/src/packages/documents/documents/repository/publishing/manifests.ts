import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS = 'Umb.Repository.Document.Publishing';

const publishingRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS,
	name: 'Document Publishing Repository',
	api: () => import('./document-publishing.repository.js'),
};

export const manifests: Array<ManifestTypes> = [publishingRepository];
