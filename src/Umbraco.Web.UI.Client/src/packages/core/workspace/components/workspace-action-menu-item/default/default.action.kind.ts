import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceActionMenuItem.Default',
	matchKind: 'default',
	matchType: 'workspaceActionMenuItem',
	manifest: {
		type: 'workspaceActionMenuItem',
		kind: 'default',
		weight: 1000,
		element: () => import('./workspace-action-menu-item.element.js'),
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
