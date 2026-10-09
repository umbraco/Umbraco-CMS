export const manifests: Array<UmbExtensionManifest> = [];

export const name = 'Umbraco.DynamicRoot';
export const extensions = [
	{
		name: 'Dynamic Root Bundle',
		alias: 'Umb.Bundle.DynamicRoot',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
