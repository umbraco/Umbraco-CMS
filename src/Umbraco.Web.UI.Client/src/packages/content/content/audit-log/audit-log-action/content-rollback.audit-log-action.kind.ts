import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_AUDIT_LOG_ACTION_CONTENT_ROLLBACK_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.AuditLogAction.ContentRollback',
	matchKind: 'contentRollback',
	matchType: 'auditLogAction',
	manifest: {
		type: 'auditLogAction',
		kind: 'contentRollback',
		element: () => import('./audit-log-action.element.js'),
		api: () => import('../../rollback/entity-action/content-rollback.action.js'),
		weight: 450,
		forEntityTypes: [],
		meta: {
			icon: 'icon-history',
			label: '#actions_rollback',
			additionalOptions: true,
			rollbackRepositoryAlias: '',
			detailRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_AUDIT_LOG_ACTION_CONTENT_ROLLBACK_KIND_MANIFEST;
