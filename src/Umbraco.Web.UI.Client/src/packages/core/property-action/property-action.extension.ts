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
	 * The group this action belongs to. Actions are still ordered by `weight`; a separator is rendered
	 * between two adjacent actions whose groups differ. Actions without a group form a group of their own.
	 * @example 'clipboard'
	 */
	group?: string;
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
