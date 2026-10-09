import { UmbExtensionItemStore } from './item.store.js';
import { UmbExtensionItemRepository } from './item.repository.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: 'Umb.Repository.ExtensionItem',
		name: 'Extension Item Repository',
		api: UmbExtensionItemRepository,
	},
	{
		type: 'itemStore',
		alias: 'Umb.Store.ExtensionItem',
		name: 'Extension Item Store',
		api: UmbExtensionItemStore,
	},
];
