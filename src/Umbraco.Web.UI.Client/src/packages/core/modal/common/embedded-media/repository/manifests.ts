export const UMB_OEMBED_REPOSITORY_ALIAS = 'Umb.Repository.OEmbed';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_OEMBED_REPOSITORY_ALIAS,
		name: 'OEmbed Repository',
		api: () => import('./oembed.repository.js'),
	},
];
