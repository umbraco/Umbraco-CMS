export const UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Item';
export const UMB_STYLESHEET_ITEM_STORE_ALIAS = 'Umb.ItemStore.Stylesheet';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS,
		name: 'Stylesheet Item Repository',
		api: () => import('./stylesheet-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.ItemStore.Stylesheet',
		name: 'Stylesheet Item Store',
		api: () => import('./stylesheet-item.store.js'),
	},
];
