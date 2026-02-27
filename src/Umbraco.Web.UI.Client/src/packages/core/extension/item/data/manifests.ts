import { UmbExtensionItemStore } from './item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: 'Umb.Repository.ExtensionItem',
		name: 'Extension Item Repository',
		api: () => import('./item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.Store.ExtensionItem',
		name: 'Extension Item Store',
		api: UmbExtensionItemStore,
	},
];
