import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
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
