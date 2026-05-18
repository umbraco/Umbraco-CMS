import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAuditLogTriggerMapping {
	operation: string;
	label: string;
}

export interface ManifestAuditLogTrigger extends ManifestBase {
	type: 'auditLogTrigger';
	forTriggerSource: string;
	meta: MetaAuditLogTrigger;
}

export interface MetaAuditLogTrigger {
	labels: UmbAuditLogTriggerMapping[];
	fallbackLabel?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogTrigger: ManifestAuditLogTrigger;
	}
}
