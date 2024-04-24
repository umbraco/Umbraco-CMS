import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.Document.Reference';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
	name: 'Document Reference Repository',
	api: () => import('./document-reference.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
