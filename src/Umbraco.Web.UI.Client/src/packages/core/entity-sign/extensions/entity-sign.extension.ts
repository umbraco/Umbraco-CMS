import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntitySignElement } from './entity-sign-element.interface';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntitySign<MetaType extends MetaEntitySign = MetaEntitySign>
	extends ManifestElement<UmbEntitySignElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entitySign';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntitySign {}

export interface ManifestEntitySignIconKind extends ManifestEntitySign<MetaEntitySignIconKind> {
	type: 'entitySign';
	kind: 'icon';
}

export interface MetaEntitySignIconKind extends MetaEntitySign {
	iconName: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntitySign: ManifestEntitySign;
		umbEntitySignIconKind: ManifestEntitySignIconKind;
	}
}
