import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS = 'Umb.Repository.Document.PublicAccess';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS,
	name: 'Document Public Access Repository',
	api: () => import('./public-access.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
