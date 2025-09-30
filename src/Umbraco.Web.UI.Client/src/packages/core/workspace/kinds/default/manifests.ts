import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Workspace.Default',
		matchKind: 'default',
		matchType: 'workspace',
		manifest: {
			type: 'workspace',
			kind: 'default',
			element: () => import('./default-workspace.element.js'),
			api: () => import('./default-workspace.context.js'),
		},
	},
];
