import type { UmbEntityActionElement } from './entity-action-element.interface.js';
import type { UmbEntityAction } from './entity-action.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction<MetaType extends MetaEntityAction = MetaEntityAction>
	extends
		ManifestElementAndApi<UmbEntityActionElement, UmbEntityAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityAction';
	forEntityTypes: Array<string>;
	/**
	 * The alias of the action group this action belongs to. The actions of a group render together between
	 * separators, ordered by their own weight, and the group's weight orders the group in the menu.
	 * @examples ["Umb.EntityActionGroup.Structure"]
	 */
	group?: string;
	/**
	 * Renders a separator above this action, also inside its group, unless it is the first action in the menu.
	 * The separator belongs to this action, so it is not rendered when the action is not.
	 */
	separatorBefore?: boolean;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityAction: ManifestEntityAction;
	}
}
