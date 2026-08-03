import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import type { BlockWorkspaceHasContentConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceHasContentCondition
	extends UmbConditionBase<BlockWorkspaceHasContentConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockWorkspaceHasContentConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.hasContent,
				(hasContent) => {
					this.permitted = hasContent === true;
				},
				'observeHasContent',
			);
		});
	}
}

export default UmbBlockWorkspaceHasContentCondition;
