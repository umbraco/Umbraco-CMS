export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.LanguageRoot',
		name: 'Language Root Workspace',
		element: () => import('./language-root-workspace.element.js'),
		meta: {
			entityType: 'language-root',
		},
	},
];
