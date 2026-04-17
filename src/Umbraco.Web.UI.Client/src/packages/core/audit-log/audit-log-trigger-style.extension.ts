import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface AuditLogTriggerMapping {
	operation: string;
	label: string;
}

export interface ManifestAuditLogTriggerStyle extends ManifestBase {
	type: 'auditLogTriggerStyle';
	forTriggerSource: string;
	meta: MetaAuditLogTriggerStyle;
}

export interface MetaAuditLogTriggerStyle {
	mappings: AuditLogTriggerMapping[];
	fallbackLabel?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogTriggerStyle: ManifestAuditLogTriggerStyle;
	}
}
