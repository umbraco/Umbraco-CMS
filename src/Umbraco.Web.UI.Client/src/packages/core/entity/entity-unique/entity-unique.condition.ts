import type { UmbEntityUniqueConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEntityUniqueCondition
	extends UmbConditionBase<UmbEntityUniqueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbEntityUniqueConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(
				context?.unique,
				(unique) => {
					this.permitted = unique === this.config.match;
				},
				'umbEntityUniqueObserver',
			);
		});
	}
}

export { UmbEntityUniqueCondition as api };
