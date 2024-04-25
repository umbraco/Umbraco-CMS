import { UmbDocumentCollectionRepository } from './document-collection.repository.js';
import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS } from './index.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionRepositoryManifest: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
	name: 'Document Collection Repository',
	api: UmbDocumentCollectionRepository,
};

export const manifests: Array<ManifestTypes> = [collectionRepositoryManifest];
