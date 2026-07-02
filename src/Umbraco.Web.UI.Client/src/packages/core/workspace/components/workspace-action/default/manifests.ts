import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbWorkspaceActionElement from './workspace-action.element.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceAction.Default',
	matchKind: 'default',
	matchType: 'workspaceAction',
	manifest: {
		type: 'workspaceAction',
		kind: 'default',
		weight: 1000,
		element: UmbWorkspaceActionElement,
		meta: {
			label: '(Missing label in manifest)',
		},
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [manifest];
