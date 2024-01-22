import { UmbDocumentDetailRepository } from './document-detail.repository.js';
import { UmbDocumentDetailStore } from './document-detail.store.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Document.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
	name: 'Document Detail Repository',
	api: UmbDocumentDetailRepository,
};

export const UMB_DOCUMENT_DETAIL_STORE_ALIAS = 'Umb.Store.Document.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_DETAIL_STORE_ALIAS,
	name: 'Document Detail Store',
	api: UmbDocumentDetailStore,
};

export const manifests = [repository, store];
