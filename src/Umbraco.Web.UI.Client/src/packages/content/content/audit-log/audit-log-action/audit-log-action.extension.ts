import type { UmbEntityActionElement, UmbEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestAuditLogAction<MetaType extends MetaAuditLogAction = MetaAuditLogAction>
	extends ManifestElementAndApi<UmbEntityActionElement, UmbEntityAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'auditLogAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaAuditLogAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbAuditLogAction: ManifestAuditLogAction;
	}
}
