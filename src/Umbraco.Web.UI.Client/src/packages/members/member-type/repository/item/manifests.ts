export const UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberTypeItem';
export const UMB_MEMBER_TYPE_STORE_ALIAS = 'Umb.Store.MemberTypeItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Member Type Item Repository',
		api: () => import('./member-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_TYPE_STORE_ALIAS,
		name: 'Member Type Item Store',
		api: () => import('./member-type-item.store.js'),
	},
];
