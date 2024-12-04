export const UMB_CULTURE_REPOSITORY_ALIAS = 'Umb.Repository.Culture';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CULTURE_REPOSITORY_ALIAS,
		name: 'Cultures Repository',
		api: () => import('./culture.repository.js'),
	},
];
