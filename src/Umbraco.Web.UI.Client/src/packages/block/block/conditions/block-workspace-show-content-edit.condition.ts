import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import type { BlockWorkspaceShowContentEditConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceShowContentEditCondition
	extends UmbConditionBase<BlockWorkspaceShowContentEditConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockWorkspaceShowContentEditConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.showContentEdit,
				(showContentEdit) => {
					this.permitted = showContentEdit ?? false;
				},
				'observeShowContentEdit',
			);
		});
	}
}

export default UmbBlockWorkspaceShowContentEditCondition;
