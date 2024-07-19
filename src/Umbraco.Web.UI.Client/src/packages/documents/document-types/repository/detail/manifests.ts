import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Detail';
export const UMB_DOCUMENT_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.DocumentType.Detail';

const detailRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Document Types Repository',
	api: () => import('./document-type-detail.repository.js'),
};

const detailStore: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_TYPE_DETAIL_STORE_ALIAS,
	name: 'Document Type Store',
	api: () => import('./document-type-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [detailRepository, detailStore];
