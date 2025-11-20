import { UMB_IS_TRASHED_ENTITY_CONTEXT } from '../../constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbIsTrashedCondition extends UmbConditionBase<UmbConditionConfigBase> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_IS_TRASHED_ENTITY_CONTEXT, (context) => {
			this.observe(context?.isTrashed, (isTrashed) => {
				this.permitted = isTrashed === true;
			});
		});
	}
}

export { UmbIsTrashedCondition as api };
