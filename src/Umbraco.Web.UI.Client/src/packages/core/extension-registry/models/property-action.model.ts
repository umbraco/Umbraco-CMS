import type { ConditionTypes } from '../conditions/types.js';
import type { UmbPropertyAction } from '../../property-action/components/property-action/property-action.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyAction<MetaType extends MetaPropertyAction = MetaPropertyAction>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbPropertyAction<MetaType>>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'propertyAction';
	forPropertyEditorUis: string[];
	meta: MetaType;
}

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
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon: string;

	/**
	 * The friendly name of the action to perform
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label: string;
}
