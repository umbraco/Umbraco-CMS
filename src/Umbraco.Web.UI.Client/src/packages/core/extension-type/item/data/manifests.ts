import { UmbExtensionTypeItemStore } from './item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: 'Umb.Repository.ExtensionTypeItem',
		name: 'Extension Type Item Repository',
		api: () => import('./item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.Store.ExtensionTypeItem',
		name: 'Extension Type Item Store',
		api: UmbExtensionTypeItemStore,
	},
];
