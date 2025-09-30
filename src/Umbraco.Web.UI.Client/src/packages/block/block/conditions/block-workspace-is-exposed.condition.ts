import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../index.js';
import type { BlockEntryIsExposedConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsExposedCondition
	extends UmbConditionBase<BlockEntryIsExposedConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<BlockEntryIsExposedConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.exposed,
				(exposed) => {
					if (exposed !== undefined) {
						// Check if equal to match, if match not set it defaults to true.
						this.permitted = exposed === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeHasExpose',
			);
		});
	}
}

export default UmbBlockEntryIsExposedCondition;
