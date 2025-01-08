import { UMB_PROPERTY_CONTEXT } from '../../components/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyHasValueCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context.value, (value) => {
				this.permitted = value !== undefined;
			});
		});
	}
}

export { UmbPropertyHasValueCondition as api };
