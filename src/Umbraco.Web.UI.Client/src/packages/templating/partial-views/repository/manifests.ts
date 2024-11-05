import { manifests as itemManifests } from './item/manifests.js';

export const UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Detail';
export const UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS = 'Umb.Store.PartialView.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
		name: 'Partial View Detail Repository',
		api: () => import('./partial-view-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS,
		name: 'Partial View Detail Store',
		api: () => import('./partial-view-detail.store.js'),
	},
	...itemManifests,
];
