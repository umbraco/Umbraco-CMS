import type { ManifestWorkspaceInfoApp, MetaWorkspaceInfoApp } from '@umbraco-cms/backoffice/workspace';

export type {
	UmbAuditLogTagData,
	UmbAuditLogTagLocalizeKeys,
	UmbAuditLogTagStyleMap,
} from '@umbraco-cms/backoffice/audit-log';

export interface MetaWorkspaceInfoAppAuditLogKind extends MetaWorkspaceInfoApp {
	auditLogRepositoryAlias: string;
}

export interface ManifestWorkspaceInfoAppAuditLogKind
	extends ManifestWorkspaceInfoApp<MetaWorkspaceInfoAppAuditLogKind> {
	type: 'workspaceInfoApp';
	kind: 'auditLog';
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceInfoAppAuditLogKind: ManifestWorkspaceInfoAppAuditLogKind;
	}
}
