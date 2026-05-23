import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface MetaAuditLogType {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';

	/**
	 * The badge label. Passed through the localization system, so this may be either a localization key
	 * (e.g. `auditTrails_smallSave`) or a direct string (which is returned unchanged when no key matches).
	 */
	label: string;

	/**
	 * Optional description used for the comment column when the audit entry is custom or has parameters.
	 * Like `label`, this is passed through the localization system: supply a localization key for
	 * localized text, or a direct string for verbatim display.
	 */
	desc?: string;
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
