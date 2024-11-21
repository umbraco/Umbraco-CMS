export const UMB_MEDIA_VALIDATION_REPOSITORY_ALIAS = 'Umb.Repository.Document.Validation';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_VALIDATION_REPOSITORY_ALIAS,
		name: 'Media Validation Repository',
		api: () => import('./media-validation.repository.js'),
	},
];
