import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../../entity-detail/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbContentWorkspaceIsLoadedCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.loading.isOn,
				(isLoading) => {
					// permitted is true when NOT loading (loading complete)
					this.permitted = isLoading === false;
				},
				'isLoadedObserver',
			);
		});
	}
}

export { UmbContentWorkspaceIsLoadedCondition as api };
