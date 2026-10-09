import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export class UmbCurrentUserDocumentBlueprintAccessCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			if (!context) {
				return;
			}

			// A user scoped to a container has no root access but still needs to reach the tree.
			this.observe(
				observeMultiple([context.hasDocumentBlueprintRootAccess, context.documentBlueprintStartNodeUniques]),
				([hasRootAccess, startNodeUniques]) => {
					this.permitted = hasRootAccess === true || (startNodeUniques?.length ?? 0) > 0;
				},
			);
		});
	}
}

export { UmbCurrentUserDocumentBlueprintAccessCondition as api };
