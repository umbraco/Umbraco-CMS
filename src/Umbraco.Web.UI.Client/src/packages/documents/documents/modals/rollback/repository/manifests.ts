export const UMB_ROLLBACK_REPOSITORY_ALIAS = 'Umb.Repository.Rollback';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ROLLBACK_REPOSITORY_ALIAS,
		name: 'Rollback Repository',
		api: () => import('./rollback.repository.js'),
	},
];
