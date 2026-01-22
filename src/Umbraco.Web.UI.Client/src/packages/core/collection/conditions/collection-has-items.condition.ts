import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { CollectionAliasConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCollectionHasItemsCondition
	extends UmbConditionBase<CollectionAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<CollectionAliasConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.observe(context?.totalItems, (totalItems) => {
				this.permitted = !!totalItems && totalItems > 0;
			});
		});
	}
}

export { UmbCollectionHasItemsCondition as api };
