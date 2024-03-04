import { UMB_DEFAULT_COLLECTION_CONTEXT } from './default/collection-default.context.js';
import type { CollectionBulkActionPermissionConditionConfig } from './collection-bulk-action-permission.manifest.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbCollectionBulkActionPermissionCondition
	extends UmbConditionBase<CollectionBulkActionPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(args: UmbConditionControllerArguments<CollectionBulkActionPermissionConditionConfig>) {
		super(args);

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (context) => {
			const allowedActions = context.getConfig().allowedEntityBulkActions;
			this.permitted = allowedActions ? this.config.match(allowedActions) : false;
		});
	}
}

export default UmbCollectionBulkActionPermissionCondition;
