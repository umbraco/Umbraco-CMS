import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface MetaAuditLogTrigger {
	label: string;
}

export interface ManifestAuditLogTrigger extends ManifestBase {
	type: 'auditLogTrigger';
	forTriggerSource: string;
	forTriggerOperation?: string;
	meta: MetaAuditLogTrigger;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogTrigger: ManifestAuditLogTrigger;
	}
}
