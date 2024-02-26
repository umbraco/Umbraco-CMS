import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Template.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
	name: 'Template Detail Repository',
	api: () => import('./template-detail.repository.js'),
};

export const UMB_TEMPLATE_DETAIL_STORE_ALIAS = 'Umb.Store.Template.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_TEMPLATE_DETAIL_STORE_ALIAS,
	name: 'Template Detail Store',
	api: () => import('./template-detail.store.js'),
};

export const manifests = [repository, store];
