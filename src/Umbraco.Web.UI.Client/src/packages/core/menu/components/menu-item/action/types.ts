import type { MetaMenuItem } from '../menu-item.extension.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbAction } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbMenuItemActionApiArgs<MetaArgsType> {
	meta: MetaArgsType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMenuItemActionApi<ArgsMetaType = never> extends UmbAction<UmbMenuItemActionApiArgs<ArgsMetaType>> {}

export interface UmbMenuItemActionElement extends UmbControllerHostElement {
	manifest?: ManifestMenuItemActionKind;
}

// Unable to extend from `ManifestMenuItem` as `UmbMenuItemElement` is of type `HTMLElement`, but for use with an API,
// it needs to be of type `UmbControllerHostElement`, and modifying `UmbMenuItemElement` would be a breaking-change. [LK]
export interface ManifestMenuItemActionKind
	extends ManifestElementAndApi<UmbMenuItemActionElement, UmbMenuItemActionApi<MetaMenuItemActionKind>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'menuItem';
	kind: 'action';
	meta: MetaMenuItemActionKind;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaMenuItemActionKind extends Exclude<MetaMenuItem, 'entityType'> {}

declare global {
	interface UmbExtensionManifestMap {
		umbActionMenuItemKind: ManifestMenuItemActionKind;
	}
}
