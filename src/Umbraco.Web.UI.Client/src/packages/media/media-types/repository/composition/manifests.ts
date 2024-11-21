export const UMB_MEDIA_TYPE_COMPOSITION_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Composition';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		name: 'Media Type Composition Repository',
		api: () => import('./media-type-composition.repository.js'),
	},
];
