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
		console.log('BlockWorkspaceIsReadOnlyCondition init');

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, async (context) => {
			// await 2 seconds:
			//await new Promise((resolve) => setTimeout(resolve, 2000));
			this.observe(
				context?.readOnlyGuard.isPermittedForObservableVariant(context.variantId),
				(isReadOnly) => {
					if (isReadOnly !== undefined) {
						const match = this.config.match !== undefined ? args.config.match : true;
						this.permitted = isReadOnly === match;
						console.log('BlockWorkspaceIsReadOnlyCondition', this.permitted, {
							context,
							isReadOnly,
							match,
							permitted: this.permitted,
						});
					}
				},
				'observeIsReadOnly',
			);
		});
	}
}

export default UmbBlockWorkspaceIsReadOnlyCondition;
