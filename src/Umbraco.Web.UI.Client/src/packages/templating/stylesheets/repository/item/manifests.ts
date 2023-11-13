import { UmbStylesheetItemStore } from './stylesheet-item.store.js';
import { UmbStylesheetItemRepository } from './stylesheet-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const STYLESHEET_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.StylesheetItem';
const repository: ManifestRepository = {
	type: 'repository',
	alias: STYLESHEET_ITEM_REPOSITORY_ALIAS,
	name: 'Stylesheet Item Repository',
	api: UmbStylesheetItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: 'Umb.ItemStore.Stylesheet',
	name: 'Stylesheet Item Store',
	api: UmbStylesheetItemStore,
};

export const manifests = [repository, itemStore];
