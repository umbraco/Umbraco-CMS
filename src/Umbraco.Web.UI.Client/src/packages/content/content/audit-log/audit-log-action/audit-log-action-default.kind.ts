import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_AUDIT_LOG_ACTION_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.AuditLogAction.Default',
	matchKind: 'default',
	matchType: 'auditLogAction',
	manifest: {
		type: 'auditLogAction',
		kind: 'default',
		weight: 1000,
		element: () => import('./audit-log-action.element.js'),
		forEntityTypes: [],
		meta: {
			icon: '',
			label: 'Default Audit Log Action',
		},
	},
};

export const manifest = UMB_AUDIT_LOG_ACTION_DEFAULT_KIND_MANIFEST;
