import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceInfoApp.AuditLog',
	matchKind: 'auditLog',
	matchType: 'workspaceInfoApp',
	manifest: {
		type: 'workspaceInfoApp',
		kind: 'auditLog',
		element: () => import('./content-audit-log-workspace-info-app.element.js'),
		weight: 80,
	},
};
