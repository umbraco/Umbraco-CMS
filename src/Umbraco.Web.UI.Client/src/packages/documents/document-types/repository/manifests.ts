import { UmbDocumentTypeRepository } from './document-type.repository.js';
import { UmbDocumentTypeStore } from './document-type.store.js';
import { manifests as itemManifests } from './item/manifests.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
	name: 'Document Types Repository',
	api: UmbDocumentTypeRepository,
};

export const DOCUMENT_TYPE_STORE_ALIAS = 'Umb.Store.DocumentType';

const store: ManifestStore = {
	type: 'store',
	alias: DOCUMENT_TYPE_STORE_ALIAS,
	name: 'Document Type Store',
	api: UmbDocumentTypeStore,
};

export const manifests = [repository, store, ...itemManifests];
