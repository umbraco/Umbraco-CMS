import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

export class UmbAllowMediaRecycleBinCurrentUserCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.hasMediaRootAccess, (hasAccess) => {
				this.permitted = hasAccess === true;
			});
		});
	}
}

export { UmbAllowMediaRecycleBinCurrentUserCondition as api };
