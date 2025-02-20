import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.MenuBreadcrumb',
	matchKind: 'menuBreadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		element: () => import('./workspace-menu-breadcrumb.element.js'),
		weight: 1000,
	},
};
