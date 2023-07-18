import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction extends ManifestElement, ManifestWithConditions<ConditionsEntityBulkAction> {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
}

export interface MetaEntityBulkAction {
	/**
	 * A friendly label for the action
	 */
	label: string;

	/**
	 * @TJS-ignore
	 */
	api: any; // create interface

	/**
	 * The alias for the repsoitory of the entity type this action is for
	 * such as 'Umb.Repository.Documents'
	 *
	 * @examples [
	 *   "Umb.Repository.Documents"
	 * ]
	 */
	repositoryAlias: string;
}

export interface ConditionsEntityBulkAction {
	/**
	 * The entity type this action is for
	 *
	 * @examples [
	 * "document",
	 * "media",
	 * "user",
	 * "user-group"
	 * ]
	 */
	entityType: string;
}
