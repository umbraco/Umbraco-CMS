import { UMB_PICKER_CONTEXT } from '../../picker.context.token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbIsNotPickerCondition extends UmbConditionBase<UmbConditionConfigBase> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		// Default: assume NOT in picker context (permitted = true)
		this.permitted = true;

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.permitted = context === undefined;
		});
	}
}

export { UmbIsNotPickerCondition as api };
