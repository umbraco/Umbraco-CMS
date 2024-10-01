import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityAction, UmbEntityActionElement } from '@umbraco-cms/backoffice/entity-action';
import type { UmbModalToken, UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction<MetaType extends MetaEntityAction = MetaEntityAction>
	extends ManifestElementAndApi<UmbEntityActionElement, UmbEntityAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionCondition> {
	type: 'entityAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityAction {}

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

// DELETE
export interface ManifestEntityActionDeleteKind extends ManifestEntityAction<MetaEntityActionDeleteKind> {
	type: 'entityAction';
	kind: 'delete';
}

export interface MetaEntityActionDeleteKind extends MetaEntityActionDefaultKind {
	detailRepositoryAlias: string;
	itemRepositoryAlias: string;
}

export type UmbEntityActionExtensions =
	| ManifestEntityAction
	| ManifestEntityActionDefaultKind
	| ManifestEntityActionDeleteKind
	| ManifestEntityActionTrashKind;

/**
 * @deprecated use `UmbEntityActionExtensions` instead.
 */
export type ManifestEntityActions = UmbEntityActionExtensions;

declare global {
	interface UmbExtensionManifestMap {
		UmbEntityActionExtensions: UmbEntityActionExtensions;
	}
}
