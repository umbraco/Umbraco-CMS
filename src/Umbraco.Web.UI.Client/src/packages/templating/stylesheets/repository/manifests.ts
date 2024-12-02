import { manifests as itemManifests } from './item/manifests.js';

export const UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Detail';
export const UMB_STYLESHEET_DETAIL_STORE_ALIAS = 'Umb.Store.Stylesheet.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS,
		name: 'Stylesheet Detail Repository',
		api: () => import('./stylesheet-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_STYLESHEET_DETAIL_STORE_ALIAS,
		name: 'Stylesheet Detail Store',
		api: () => import('./stylesheet-detail.store.js'),
	},
	...itemManifests,
];
