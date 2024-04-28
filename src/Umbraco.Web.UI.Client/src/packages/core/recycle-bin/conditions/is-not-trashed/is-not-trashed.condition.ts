import { UMB_IS_TRASHED_CONTEXT } from '../../contexts/is-trashed/index.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbIsNotTrashedCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_IS_TRASHED_CONTEXT, (context) => {
			this.observe(context.isTrashed, (isTrashed) => {
				this.permitted = isTrashed === false;
			});
		});
	}
}

export { UmbIsNotTrashedCondition as api };
