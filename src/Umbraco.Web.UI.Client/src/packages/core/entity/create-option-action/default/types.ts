import type {
	ManifestEntityCreateOptionAction,
	MetaEntityCreateOptionAction,
} from '../entity-create-option-action.extension.js';

export interface ManifestEntityCreateOptionActionDefaultKind
	extends ManifestEntityCreateOptionAction<MetaEntityCreateOptionActionDefaultKind> {
	type: 'entityCreateOptionAction';
	kind: 'default';
}

export interface MetaEntityCreateOptionActionDefaultKind extends MetaEntityCreateOptionAction {
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
	 * @memberof MetaEntityCreateOptionActionDefaultKind
	 */
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDefaultEntityCreateOptionActionKind: ManifestEntityCreateOptionActionDefaultKind;
	}
}
