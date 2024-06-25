import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.DocumentBlueprint.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
	name: 'Document Blueprint Detail Repository',
	api: () => import('./document-blueprint-detail.repository.js'),
};

export const UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS = 'Umb.Store.DocumentBlueprint.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS,
	name: 'Document Blueprint Detail Store',
	api: () => import('./document-blueprint-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
