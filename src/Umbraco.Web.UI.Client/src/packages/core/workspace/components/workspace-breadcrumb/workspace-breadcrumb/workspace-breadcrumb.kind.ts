import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceFooterApp.Breadcrumb',
	matchKind: 'breadcrumb',
	matchType: 'workspaceFooterApp',
	manifest: {
		type: 'workspaceFooterApp',
		kind: 'breadcrumb',
		element: () => import('./workspace-breadcrumb.element.js'),
		weight: 1000,
	},
};
