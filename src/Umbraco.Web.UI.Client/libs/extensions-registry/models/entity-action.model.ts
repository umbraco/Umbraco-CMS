import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

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
	api: any; // create interface

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
	entityTypes: Array<string>;
}
