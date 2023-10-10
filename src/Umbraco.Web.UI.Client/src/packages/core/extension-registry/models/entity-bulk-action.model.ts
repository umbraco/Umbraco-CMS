import type { ConditionTypes } from '../conditions/types.js';
import type { UmbEntityBulkAction } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction extends ManifestElementAndApi<HTMLElement, UmbEntityBulkAction>, ManifestWithDynamicConditions<ConditionTypes> {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
}

export interface MetaEntityBulkAction {
	/**
	 * A friendly label for the action
	 */
	label: string;

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
