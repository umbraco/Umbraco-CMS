import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS } from './index.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionRepositoryManifest: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
	name: 'Document Collection Repository',
	api: () => import('./document-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [collectionRepositoryManifest];
