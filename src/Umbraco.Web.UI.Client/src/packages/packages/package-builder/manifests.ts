export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.PackageBuilder',
		name: 'Package Builder Workspace',
		element: () => import('./workspace/workspace-package-builder.element.js'),
		meta: {
			entityType: 'package-builder',
		},
	},
];
