import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction extends ManifestElement {
	type: 'entityAction';
	meta: MetaEntityAction;
	conditions: ConditionsEntityAction;
}

export interface MetaEntityAction {
	/**
	 * An icon to represent the action to be performed
	 *
	 * @examples [
	 *   "umb:box",
	 *   "umb:grid"
	 * ]
	 */
	icon?: string;

	/**
	 * The friendly name of the action to perform
	 *
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label: string;

	/**
	 * @TJS-ignore
	 */
	api: any; // TODO: create interface

	/**
	 * The alias for the repsoitory of the entity type this action is for
	 * such as 'Umb.Repository.Documents'
	 * @examples [
	 *   "Umb.Repository.Documents"
	 * ]
	 */
	repositoryAlias: string;
}

export interface ConditionsEntityAction {
	/**
	 * The entity types that this action can be performed on
	 * @examples [
	 * "data-type",
	 * "data-type-folder",
	 * "document",
	 * "document-root",
	 * "document-type",
	 * "dictionary-item",
	 * "language",
	 * "language-root",
	 * "member",
	 * "member-group",
	 * "member-type",
	 * "template",
	 * "template-root",
	 * "partial-view"
	 * ]
	 */
	entityTypes: Array<string>;
}
