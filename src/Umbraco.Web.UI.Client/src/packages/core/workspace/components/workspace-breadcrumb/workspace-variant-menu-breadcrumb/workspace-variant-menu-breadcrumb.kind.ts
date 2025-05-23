import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.VariantMenuBreadcrumb',
	matchKind: 'variantMenuBreadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'variantMenuBreadcrumb',
		element: () => import('./workspace-variant-menu-breadcrumb.element.js'),
		weight: 1000,
	},
};
