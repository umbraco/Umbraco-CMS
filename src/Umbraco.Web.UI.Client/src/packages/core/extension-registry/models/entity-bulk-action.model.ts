import type { ConditionTypes } from '../conditions/types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction<MetaType extends MetaEntityBulkAction>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbEntityBulkActionBase<MetaType>>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'entityBulkAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

export interface MetaEntityBulkAction {
	/**
	 * The friendly name of the action to perform
	 *
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label?: string;
}
