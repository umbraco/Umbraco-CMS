import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceView.Content.Collection',
		matchKind: 'contentCollection',
		matchType: 'workspaceView',
		manifest: {
			type: 'workspaceView',
			kind: 'contentCollection',
			element: () => import('./content-collection-workspace-view.element.js'),
			weight: 300,
			meta: {
				label: 'Collection',
				pathname: 'collection',
				icon: 'icon-grid',
			},
		},
	},
];
