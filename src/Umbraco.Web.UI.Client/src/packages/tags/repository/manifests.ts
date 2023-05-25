import { UmbTagRepository } from './tag.repository.js';
import { UmbTagStore } from './tag.store.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const TAG_REPOSITORY_ALIAS = 'Umb.Repository.Tags';

const repository: ManifestRepository = {
	type: 'repository',
	alias: TAG_REPOSITORY_ALIAS,
	name: 'Tags Repository',
	class: UmbTagRepository,
};

export const TAG_STORE_ALIAS = 'Umb.Store.Tags';

const store: ManifestStore = {
	type: 'store',
	alias: TAG_STORE_ALIAS,
	name: 'Tags Store',
	class: UmbTagStore,
};

export const manifests = [repository, store];
