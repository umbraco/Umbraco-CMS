import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import type { BlockWorkspaceContentHasPropertiesConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceContentHasPropertiesCondition
	extends UmbConditionBase<BlockWorkspaceContentHasPropertiesConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockWorkspaceContentHasPropertiesConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.content.structure.contentTypeHasProperties,
				(hasProperties) => {
					this.permitted = hasProperties ?? false;
				},
				'observeContentHasProperties',
			);
		});
	}
}

export default UmbBlockWorkspaceContentHasPropertiesCondition;
