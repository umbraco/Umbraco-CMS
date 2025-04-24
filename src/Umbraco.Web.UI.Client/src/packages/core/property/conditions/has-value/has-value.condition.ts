import { UMB_PROPERTY_CONTEXT } from '../../components/index.js';
import type { UmbPropertyHasValueConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyHasValueCondition
	extends UmbConditionBase<UmbPropertyHasValueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbPropertyHasValueConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.value, (value) => {
				this.permitted = value !== undefined;
			});
		});
	}
}

export { UmbPropertyHasValueCondition as api };
