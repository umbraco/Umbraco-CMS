export const UMB_MEMBER_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberItem';
export const UMB_MEMBER_STORE_ALIAS = 'Umb.Store.MemberItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
		name: 'Member Item Repository',
		api: () => import('./member-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_STORE_ALIAS,
		name: 'Member Item Store',
		api: () => import('./member-item.store.js'),
	},
];
