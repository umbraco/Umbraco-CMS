export const UMB_MEMBER_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Member.Detail';
export const UMB_MEMBER_DETAIL_STORE_ALIAS = 'Umb.Store.Member.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
		name: 'Member Detail Repository',
		api: () => import('./member-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEMBER_DETAIL_STORE_ALIAS,
		name: 'Member Detail Store',
		api: () => import('./member-detail.store.js'),
	},
];
