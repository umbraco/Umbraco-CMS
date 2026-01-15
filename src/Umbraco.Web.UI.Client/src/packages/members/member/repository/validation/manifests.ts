export const UMB_MEMBER_VALIDATION_REPOSITORY_ALIAS = 'Umb.Repository.Member.Validation';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_VALIDATION_REPOSITORY_ALIAS,
		name: 'Member Validation Repository',
		api: () => import('./member-validation.repository.js'),
	},
];
