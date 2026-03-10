import type { ManifestAuditLogAction, MetaAuditLogAction } from './audit-log-action.extension.js';

export type * from './audit-log-action.extension.js';

export type {
	UmbAuditLogTagData,
	UmbAuditLogTagLocalizeKeys,
	UmbAuditLogTagStyleMap,
} from '@umbraco-cms/backoffice/audit-log';

export interface MetaAuditLogActionDefaultKind extends MetaAuditLogAction {
	icon: string;
	label: string;
	additionalOptions?: boolean;
}

export interface ManifestAuditLogActionDefaultKind extends ManifestAuditLogAction<MetaAuditLogActionDefaultKind> {
	type: 'auditLogAction';
	kind: 'default';
}

declare global {
	interface UmbExtensionManifestMap {
		umbAuditLogActionDefaultKind: ManifestAuditLogActionDefaultKind;
	}
}
