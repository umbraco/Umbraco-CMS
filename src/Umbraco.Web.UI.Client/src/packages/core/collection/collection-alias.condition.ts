import { UMB_DEFAULT_COLLECTION_CONTEXT } from './default/collection-default.context.js';
import type { CollectionAliasConditionConfig } from './collection-alias.manifest.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbCollectionAliasCondition
	extends UmbConditionBase<CollectionAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(args: UmbConditionControllerArguments<CollectionAliasConditionConfig>) {
		super(args);
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (context) => {
			this.permitted = context.getManifest()?.alias === this.config.match;
		});
	}
}

export default UmbCollectionAliasCondition;
