import { UmbTemplateDetailRepository } from './template-detail.repository.js';
import { UmbTemplateDetailStore } from './template-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Template.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_TEMPLATE_DETAIL_REPOSITORY_ALIAS,
	name: 'Template Detail Repository',
	api: UmbTemplateDetailRepository,
};

export const UMB_TEMPLATE_DETAIL_STORE_ALIAS = 'Umb.Store.Template.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_TEMPLATE_DETAIL_STORE_ALIAS,
	name: 'Template Detail Store',
	api: UmbTemplateDetailStore,
};

export const manifests = [repository, store];
