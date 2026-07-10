import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

export const UMB_CURRENT_USER_ALLOW_ELEMENT_RECYCLE_BIN_CONDITION_ALIAS =
	'Umb.Condition.CurrentUser.AllowElementRecycleBin';

export class UmbAllowElementRecycleBinCurrentUserCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.hasElementRootAccess, (hasAccess) => {
				this.permitted = hasAccess === true;
			});
		});
	}
}

export { UmbAllowElementRecycleBinCurrentUserCondition as api };
