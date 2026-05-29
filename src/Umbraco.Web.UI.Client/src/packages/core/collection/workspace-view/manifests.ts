import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCollectionWorkspaceViewElement } from './collection-workspace-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceView.Collection',
		matchKind: 'collection',
		matchType: 'workspaceView',
		manifest: {
			type: 'workspaceView',
			kind: 'collection',
			element: UmbCollectionWorkspaceViewElement,
			meta: {
				label: 'Collection',
				pathname: 'collection',
				icon: 'icon-layers',
			},
		},
	},
];
