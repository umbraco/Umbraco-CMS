import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceAction.Default',
	matchKind: 'default',
	matchType: 'workspaceAction',
	manifest: {
		type: 'workspaceAction',
		kind: 'default',
		weight: 1000,
		element: () => import('./workspace-action.element.js'),
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
