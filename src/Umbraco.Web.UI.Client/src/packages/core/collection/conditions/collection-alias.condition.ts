import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { CollectionAliasConditionConfig } from './types.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCollectionAliasCondition
	extends UmbConditionBase<CollectionAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<CollectionAliasConditionConfig>) {
		super(host, args);
		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.permitted = context.manifest?.alias === this.config.match;
		});
	}
}

export default UmbCollectionAliasCondition;
