import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTreeWorkspaceViewElement } from './tree-workspace-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceView.Tree',
		matchKind: 'tree',
		matchType: 'workspaceView',
		manifest: {
			type: 'workspaceView',
			kind: 'tree',
			element: UmbTreeWorkspaceViewElement,
			meta: {
				label: 'Children',
				pathname: 'children',
				icon: 'icon-bulleted-list',
			},
		},
	},
];
