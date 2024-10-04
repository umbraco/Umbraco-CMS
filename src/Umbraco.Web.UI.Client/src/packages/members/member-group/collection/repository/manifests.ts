export const UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup.Collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS,
		name: 'Member Group Collection Repository',
		api: () => import('./member-group-collection.repository.js'),
	},
];
