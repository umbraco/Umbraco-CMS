import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface MetaAuditLogTypeStyle {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';
	label: string;
}

export interface ManifestAuditLogTypeStyle extends ManifestBase {
	type: 'auditLogTypeStyle';
	forTypeAliases: string[];
	meta: MetaAuditLogTypeStyle;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestAuditLogTypeStyle: ManifestAuditLogTypeStyle;
	}
}
