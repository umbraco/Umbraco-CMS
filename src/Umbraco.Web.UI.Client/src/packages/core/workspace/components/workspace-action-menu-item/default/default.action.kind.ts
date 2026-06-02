import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbWorkspaceActionMenuItemElement from './workspace-action-menu-item.element.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceActionMenuItem.Default',
	matchKind: 'default',
	matchType: 'workspaceActionMenuItem',
	manifest: {
		type: 'workspaceActionMenuItem',
		kind: 'default',
		weight: 1000,
		element: UmbWorkspaceActionMenuItemElement,
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
