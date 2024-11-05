export const UMB_SCRIPT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Script.Item';
export const UMB_SCRIPT_ITEM_STORE_ALIAS = 'Umb.ItemStore.Script';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
		name: 'Script Item Repository',
		api: () => import('./script-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.ItemStore.Script',
		name: 'Script Item Store',
		api: () => import('./script-item.store.js'),
	},
];
