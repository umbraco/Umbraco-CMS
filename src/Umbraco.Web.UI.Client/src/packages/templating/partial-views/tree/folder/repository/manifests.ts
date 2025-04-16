export const UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Folder';
export const UMB_PARTIAL_VIEW_FOLDER_STORE_ALIAS = 'Umb.Store.PartialView.Folder';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
		name: 'Partial View Folder Repository',
		api: () => import('./partial-view-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_PARTIAL_VIEW_FOLDER_STORE_ALIAS,
		name: 'Partial View Folder Store',
		api: () => import('./partial-view-folder.store.js'),
	},
];
