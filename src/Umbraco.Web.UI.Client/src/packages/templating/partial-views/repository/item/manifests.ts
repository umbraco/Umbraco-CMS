export const UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Item';
export const UMB_PARTIAL_VIEW_ITEM_STORE_ALIAS = 'Umb.ItemStore.PartialView';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
		name: 'Partial View Item Repository',
		api: () => import('./partial-view-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.ItemStore.PartialView',
		name: 'Partial View Item Store',
		api: () => import('./partial-view-item.store.js'),
	},
];
