import type { ManifestEntityAction, MetaEntityAction } from '../entity-action.extension.js';

export interface ManifestEntityActionDefaultKind extends ManifestEntityAction<MetaEntityActionDefaultKind> {
	type: 'entityAction';
	kind: 'default';
}

export interface MetaEntityActionDefaultKind extends MetaEntityAction {
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

	/**
	 * The action requires additional input from the user.
	 * A dialog will prompt the user for more information or to make a choice.
	 * @type {boolean}
	 * @memberof MetaEntityActionDefaultKind
	 */
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDefaultEntityActionKind: ManifestEntityActionDefaultKind;
	}
}
