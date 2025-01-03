export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'section',
		alias: 'MyBundle.Section.Custom',
		name: 'Custom Section',
		js: '/App_Plugins/section.js',
		weight: 1,
		meta: {
			label: 'My Bundle Section',
			pathname: 'my-custom-bundle',
		},
	},
];
