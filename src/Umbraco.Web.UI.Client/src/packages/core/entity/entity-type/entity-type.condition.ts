import { UMB_ENTITY_CONTEXT } from '../entity.context-token.js';
import type { UmbEntityTypeConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEntityTypeCondition
	extends UmbConditionBase<UmbEntityTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbEntityTypeConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(context?.entityType, (entityType) => this.#check(entityType), 'umbEntityTypeObserver');
		});
	}

	#check(value: string | undefined) {
		if (!value) {
			this.permitted = false;
			return;
		}

		// if the config has a match, we only check that
		if (this.config.match) {
			this.permitted = value === this.config.match;
			return;
		}

		this.permitted = this.config.oneOf?.some((configValue) => configValue === value) ?? false;
	}
}

export { UmbEntityTypeCondition as api };
