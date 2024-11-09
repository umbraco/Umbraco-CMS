export const UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.MemberType.Detail';
export const UMB_MEMBER_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.MemberType.Detail';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Member Type Detail Repository',
		api: () => import('./member-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEMBER_TYPE_DETAIL_STORE_ALIAS,
		name: 'Member Type Detail Store',
		api: () => import('./member-type-detail.store.js'),
	},
];
