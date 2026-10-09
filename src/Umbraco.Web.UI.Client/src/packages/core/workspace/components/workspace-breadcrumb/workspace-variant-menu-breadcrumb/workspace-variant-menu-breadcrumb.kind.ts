import UmbWorkspaceVariantMenuBreadcrumbElement from './workspace-variant-menu-breadcrumb.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.VariantMenuBreadcrumb',
	matchKind: 'variantMenuBreadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'variantMenuBreadcrumb',
		element: UmbWorkspaceVariantMenuBreadcrumbElement,
		weight: 1000,
	},
};
