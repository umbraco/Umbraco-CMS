import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.ExtensionCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
	name: 'Extension Collection Repository',
	api: () => import('./extension-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
