import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { UmbCollectionHasItemsConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCollectionHasItemsCondition
	extends UmbConditionBase<UmbCollectionHasItemsConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbCollectionHasItemsConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.observe(context?.totalItems, (totalItems) => {
				this.permitted = !!totalItems && totalItems > 0;
			});
		});
	}
}
