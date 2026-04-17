import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAuditLogTriggerMapping {
	operation: string;
	label: string;
}

export interface ManifestAuditLogTriggerStyle extends ManifestBase {
	type: 'auditLogTriggerStyle';
	forTriggerSource: string;
	meta: MetaAuditLogTriggerStyle;
}

export interface MetaAuditLogTriggerStyle {
	mappings: UmbAuditLogTriggerMapping[];
	fallbackLabel?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogTriggerStyle: ManifestAuditLogTriggerStyle;
	}
}
