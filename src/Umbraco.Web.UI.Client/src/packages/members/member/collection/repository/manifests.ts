export const UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Member.Collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS,
		name: 'Member Collection Repository',
		api: () => import('./member-collection.repository.js'),
	},
];
