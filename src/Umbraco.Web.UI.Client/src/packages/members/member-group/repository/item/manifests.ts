export const UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroupItem';
export const UMB_MEMBER_GROUP_STORE_ALIAS = 'Umb.Store.MemberGroupItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
		name: 'Member Group Item Repository',
		api: () => import('./member-group-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_GROUP_STORE_ALIAS,
		name: 'Member Group Item Store',
		api: () => import('./member-group-item.store.js'),
	},
];
