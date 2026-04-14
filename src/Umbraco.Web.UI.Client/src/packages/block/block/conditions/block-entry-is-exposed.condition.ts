import { UMB_BLOCK_ENTRY_CONTEXT } from '../index.js';
import type { UmbBlockEntryIsExposedConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsExposedCondition
	extends UmbConditionBase<UmbBlockEntryIsExposedConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbBlockEntryIsExposedConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.hasExpose,
				(exposed) => {
					if (exposed !== undefined) {
						this.permitted = exposed === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeIsExposed',
			);
		});
	}
}

export default UmbBlockEntryIsExposedCondition;
