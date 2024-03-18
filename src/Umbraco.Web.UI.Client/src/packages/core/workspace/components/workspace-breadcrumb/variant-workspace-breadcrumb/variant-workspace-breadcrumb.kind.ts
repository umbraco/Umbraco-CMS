import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.VariantBreadcrumb',
	matchKind: 'variantBreadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'variantBreadcrumb',
		element: () => import('./variant-workspace-breadcrumb.element.js'),
		weight: 1000,
	},
};
