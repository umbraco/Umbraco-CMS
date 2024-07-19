import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Document.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
	name: 'Document Detail Repository',
	api: () => import('./document-detail.repository.js'),
};

export const UMB_DOCUMENT_DETAIL_STORE_ALIAS = 'Umb.Store.Document.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_DETAIL_STORE_ALIAS,
	name: 'Document Detail Store',
	api: () => import('./document-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
