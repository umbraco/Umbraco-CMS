import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface MetaAuditLogType {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';
	label: string;
}

export interface ManifestAuditLogType extends ManifestBase {
	type: 'auditLogType';
	forLogTypeAliases: string[];
	meta: MetaAuditLogType;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogType: ManifestAuditLogType;
	}
}
