import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../index.js';
import type { BlockWorkspaceIsReadOnlyConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceIsReadOnlyCondition
	extends UmbConditionBase<BlockWorkspaceIsReadOnlyConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<BlockWorkspaceIsReadOnlyConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.readOnlyGuard.permitted,
				(isReadOnly) => {
					if (isReadOnly !== undefined) {
						this.permitted = isReadOnly === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeIsReadOnly',
			);
		});
	}
}

export default UmbBlockWorkspaceIsReadOnlyCondition;
