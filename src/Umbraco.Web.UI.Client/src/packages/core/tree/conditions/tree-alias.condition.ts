import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { TreeAliasConditionConfig } from './types.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTreeAliasCondition extends UmbConditionBase<TreeAliasConditionConfig> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<TreeAliasConditionConfig>) {
		super(host, args);
		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.permitted = context?.manifest?.alias === this.config.match;
		});
	}
}

export default UmbTreeAliasCondition;
