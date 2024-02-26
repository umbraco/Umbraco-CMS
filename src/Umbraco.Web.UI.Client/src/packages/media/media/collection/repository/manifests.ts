import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS } from './index.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const collectionRepositoryManifest: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
	name: 'Media Collection Repository',
	api: () => import('./media-collection.repository.js'),
};

export const manifests = [collectionRepositoryManifest];
