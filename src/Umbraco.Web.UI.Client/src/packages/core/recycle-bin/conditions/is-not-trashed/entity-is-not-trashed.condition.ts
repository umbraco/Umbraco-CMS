import { UMB_IS_TRASHED_ENTITY_CONTEXT } from '../../constants.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbEntityIsNotTrashedCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		// this is a "negative/not" condition, so we assume the default value is that the context is not trashed
		// and therefore the condition is permitted
		this.permitted = true;

		this.consumeContext(UMB_IS_TRASHED_ENTITY_CONTEXT, (context) => {
			this.observe(context.isTrashed, (isTrashed) => {
				this.permitted = isTrashed === false;
			});
		});
	}
}

export { UmbEntityIsNotTrashedCondition as api };
