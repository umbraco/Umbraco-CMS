import { UmbLanguageDetailRepository } from './language-detail.repository.js';
import { UmbLanguageDetailStore } from './language-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Language.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
	name: 'Language Detail Repository',
	api: UmbLanguageDetailRepository,
};

export const UMB_LANGUAGE_DETAIL_STORE_ALIAS = 'Umb.Store.Language.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_LANGUAGE_DETAIL_STORE_ALIAS,
	name: 'Language Detail Store',
	api: UmbLanguageDetailStore,
};

export const manifests = [repository, store];
