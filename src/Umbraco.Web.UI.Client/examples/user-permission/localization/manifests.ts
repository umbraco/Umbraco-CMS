export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'localization',
		alias: 'Example.UserPermission.Localization.En',
		name: 'en-US User Permission Localization Example',
		js: () => import('./en.js'),
		weight: 1,
		meta: {
			culture: 'en',
		},
	},
];
