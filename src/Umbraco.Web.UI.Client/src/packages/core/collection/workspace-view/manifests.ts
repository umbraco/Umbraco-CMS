import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceView.Collection',
		matchKind: 'collection',
		matchType: 'workspaceView',
		manifest: {
			type: 'workspaceView',
			kind: 'collection',
			element: () => import('./collection-workspace-view.element.js'),
			meta: {
				label: 'Collection',
				pathname: 'collection',
				icon: 'icon-layers',
			},
		},
	},
];
