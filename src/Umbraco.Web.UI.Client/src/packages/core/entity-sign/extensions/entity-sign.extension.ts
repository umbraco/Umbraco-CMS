import type { UmbEntitySignElement } from './entity-sign-element.interface.js';
import type { UmbEntitySignApi } from './entity-sign-api.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntitySign<MetaType extends MetaEntitySign = MetaEntitySign>
	extends ManifestElementAndApi<UmbEntitySignElement, UmbEntitySignApi>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entitySign';
	forEntityTypes?: Array<string>;
	forEntityFlags?: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntitySign {}

declare global {
	interface UmbExtensionManifestMap {
		umbEntitySign: ManifestEntitySign;
	}
}
