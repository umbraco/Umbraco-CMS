import { UMB_DOCUMENT_URL_REPOSITORY_ALIAS, UMB_DOCUMENT_URL_STORE_ALIAS } from './constants.js';
import type { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const urlRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_URL_REPOSITORY_ALIAS,
	name: 'Document Url Repository',
	api: () => import('./document-url.repository.js'),
};

const urlStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DOCUMENT_URL_STORE_ALIAS,
	name: 'Document Url Store',
	api: () => import('./document-url.store.js'),
};

export const manifests = [urlRepository, urlStore];
