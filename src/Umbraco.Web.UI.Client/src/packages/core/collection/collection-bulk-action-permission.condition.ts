import { UMB_COLLECTION_CONTEXT } from './default/index.js';
import type { CollectionBulkActionPermissionConditionConfig } from './collection-bulk-action-permission.manifest.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
			const allowedActions = context.getConfig()?.allowedEntityBulkActions;
			this.permitted = allowedActions ? this.config.match(allowedActions) : false;
		});
	}
}

export default UmbCollectionBulkActionPermissionCondition;
