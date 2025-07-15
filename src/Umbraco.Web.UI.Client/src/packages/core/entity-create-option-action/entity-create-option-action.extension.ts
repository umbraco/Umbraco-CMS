import type { UmbEntityCreateOptionAction } from './entity-create-option-action.interface.js';
import type { UMB_EXTENSION_TYPE_ENTITY_CREATE_OPTION_ACTION } from './constants.js';
import type { ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityCreateOptionAction<
	MetaType extends MetaEntityCreateOptionAction = MetaEntityCreateOptionAction,
> extends ManifestApi<UmbEntityCreateOptionAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: typeof UMB_EXTENSION_TYPE_ENTITY_CREATE_OPTION_ACTION;
	forEntityTypes: Array<string>;
	meta: MetaType;
}

export interface MetaEntityCreateOptionAction {
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
	 *   "Create with Template",
	 *   "Create from Blueprint"
	 * ]
	 */
	label: string;

	/**
	 * A description of the action to be performed
	 * @examples [
	 *   "Create a document type with a template",
	 *   "Create a document from a blueprint"
	 * ]
	 */
	description?: string;

	/**
	 * The action requires additional input from the user.
	 * A dialog will prompt the user for more information or to make a choice.
	 * @type {boolean}
	 */
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityCreateOptionAction: ManifestEntityCreateOptionAction;
	}
}
