import type { ManifestWorkspaceInfoApp, MetaWorkspaceInfoApp } from '@umbraco-cms/backoffice/workspace';

export interface UmbAuditLogTagStyleMap {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';
}

export interface UmbAuditLogTagLocalizeKeys {
	label: string;
	desc: string;
}

export interface UmbAuditLogTagData {
	style: UmbAuditLogTagStyleMap;
	text: UmbAuditLogTagLocalizeKeys;
}

export interface MetaWorkspaceInfoAppAuditLogKind extends MetaWorkspaceInfoApp {
	auditLogRepositoryAlias: string;
	allowedActions?: Array<string>;
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
