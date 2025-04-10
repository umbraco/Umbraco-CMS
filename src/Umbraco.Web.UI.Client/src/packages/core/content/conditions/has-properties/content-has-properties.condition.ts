import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../../constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbContentHasPropertiesWorkspaceCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.structure.contentTypeHasProperties,
				(hasProperties) => {
					this.permitted = hasProperties ?? false;
				},
				'hasPropertiesObserver',
			);
		});
	}
}

export { UmbContentHasPropertiesWorkspaceCondition as api };
