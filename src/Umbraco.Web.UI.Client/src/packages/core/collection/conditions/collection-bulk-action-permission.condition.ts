import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { CollectionBulkActionPermissionConditionConfig } from './types.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export class UmbCollectionBulkActionPermissionCondition
	extends UmbConditionBase<CollectionBulkActionPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<CollectionBulkActionPermissionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			const allowedActions = context?.getConfig()?.allowedEntityBulkActions;
			this.permitted = allowedActions ? this.config.match(allowedActions) : false;
		});
	}
}

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export default UmbCollectionBulkActionPermissionCondition;
