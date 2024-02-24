import { UMB_DEFAULT_COLLECTION_CONTEXT } from './default/collection-default.context.js';
import type { CollectionBulkActionPermissionConditionConfig } from './collection-bulk-action-permission.manifest.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbCollectionBulkActionPermissionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: CollectionBulkActionPermissionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<CollectionBulkActionPermissionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (context) => {
			const allowedActions = context.getConfig().allowedEntityBulkActions;
			this.permitted = allowedActions ? this.config.match(allowedActions) : false;
			this.#onChange();
		});
	}
}

export default UmbCollectionBulkActionPermissionCondition;
