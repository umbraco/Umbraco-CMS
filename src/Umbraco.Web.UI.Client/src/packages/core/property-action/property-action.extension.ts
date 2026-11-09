import type { UmbPropertyAction } from './property-action.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyAction<MetaType extends MetaPropertyAction = MetaPropertyAction>
	extends
		ManifestElementAndApi<UmbControllerHostElement, UmbPropertyAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'propertyAction';
	forPropertyEditorUis: string[];
	/**
	 * Renders a separator above this action, unless it is the first action in the list.
	 * The separator belongs to this action, so it is not rendered when the action is not.
	 */
	separatorBefore?: boolean;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaPropertyAction {}

export interface ManifestPropertyActionDefaultKind<
	MetaType extends MetaPropertyActionDefaultKind = MetaPropertyActionDefaultKind,
> extends ManifestPropertyAction<MetaType> {
	type: 'propertyAction';
	kind: 'default';
}

export interface MetaPropertyActionDefaultKind extends MetaPropertyAction {
	/**
	 * An icon to represent the action to be performed
	 * @examples ["icon-box", "icon-grid"]
	 */
	icon: string;

	/**
	 * The friendly name of the action to perform
	 * @examples ["Create", "Create Content Template"]
	 */
	label: string;
}

export type UmbPropertyActionExtensions = ManifestPropertyAction | ManifestPropertyActionDefaultKind;

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyActionExtensions: UmbPropertyActionExtensions;
	}
}
