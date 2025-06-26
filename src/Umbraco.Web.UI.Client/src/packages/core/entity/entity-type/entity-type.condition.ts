import type { UmbEntityTypeConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEntityTypeCondition
	extends UmbConditionBase<UmbEntityTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbEntityTypeConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(
				context?.entityType,
				(entityType) => {
					this.permitted = entityType === this.config.match;
				},
				'umbEntityTypeObserver',
			);
		});
	}
}

export { UmbEntityTypeCondition as api };
