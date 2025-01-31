import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityAction, UmbEntityActionElement } from '@umbraco-cms/backoffice/entity-action';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction<MetaType extends MetaEntityAction = MetaEntityAction>
	extends ManifestElementAndApi<UmbEntityActionElement, UmbEntityAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityAction: ManifestEntityAction;
	}
}
