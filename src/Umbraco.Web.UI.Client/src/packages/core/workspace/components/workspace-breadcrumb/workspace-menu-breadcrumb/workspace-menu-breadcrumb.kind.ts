import UmbWorkspaceBreadcrumbElement from './workspace-menu-breadcrumb.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.MenuBreadcrumb',
	matchKind: 'menuBreadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		element: UmbWorkspaceBreadcrumbElement,
		weight: 1000,
	},
};
