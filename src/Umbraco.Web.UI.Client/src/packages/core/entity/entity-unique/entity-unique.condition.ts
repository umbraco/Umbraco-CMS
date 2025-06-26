import { UMB_ENTITY_CONTEXT } from '../entity.context-token.js';
import type { UmbEntityUnique } from '../types.js';
import type { UmbEntityUniqueConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEntityUniqueCondition
	extends UmbConditionBase<UmbEntityUniqueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbEntityUniqueConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(context?.unique, (unique) => this.#check(unique), 'umbEntityUniqueObserver');
		});
	}

	#check(value: UmbEntityUnique | undefined) {
		if (value === undefined) {
			this.permitted = false;
			return;
		}

		// if the config has a match, we only check that
		if (this.config.match !== undefined) {
			this.permitted = value === this.config.match;
			return;
		}

		this.permitted = this.config.oneOf?.some((configValue) => configValue === value) ?? false;
	}
}

export { UmbEntityUniqueCondition as api };
