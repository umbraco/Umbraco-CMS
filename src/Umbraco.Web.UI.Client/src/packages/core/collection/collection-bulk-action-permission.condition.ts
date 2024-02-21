import type { UmbCollectionBulkActionPermissions } from './types.js';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from './default/collection-default.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

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

export type CollectionBulkActionPermissionConditionConfig = UmbConditionConfigBase<
	typeof UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION
> & {
	match: (permissions: UmbCollectionBulkActionPermissions) => boolean;
};

export const UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION = 'Umb.Condition.CollectionBulkActionPermission';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Collection Bulk Action Permission Condition',
	alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
	api: UmbCollectionBulkActionPermissionCondition,
};
