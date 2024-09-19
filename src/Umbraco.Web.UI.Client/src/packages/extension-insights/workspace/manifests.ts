export const UMB_EXTENSION_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.ExtensionRoot';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.ExtensionRoot',
		name: 'Extension Root Workspace',
		element: () => import('./extension-root-workspace.element.js'),
		meta: {
			entityType: 'extension-root',
		},
	},
];
