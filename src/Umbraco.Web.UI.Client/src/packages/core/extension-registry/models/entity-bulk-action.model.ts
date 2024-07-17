import type { ConditionTypes } from '../conditions/types.js';
import type { UmbEntityBulkActionElement } from '../../entity-bulk-action/entity-bulk-action-element.interface.js';
import type { UmbEntityBulkAction } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction<MetaType extends MetaEntityBulkAction = MetaEntityBulkAction>
	extends ManifestElementAndApi<UmbEntityBulkActionElement, UmbEntityBulkAction<MetaType>>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'entityBulkAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

export interface MetaEntityBulkAction {}

export interface ManifestEntityBulkActionDefaultKind extends ManifestEntityBulkAction<MetaEntityBulkActionDefaultKind> {
	type: 'entityBulkAction';
	kind: 'default';
}

export interface MetaEntityBulkActionDefaultKind extends MetaEntityBulkAction {
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
