export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.Package',
		name: 'Package Workspace',
		element: () => import('./workspace/workspace-package.element.js'),
		meta: {
			entityType: 'package',
		},
	},
];
