import { UMB_HAS_CHILDREN_ENTITY_CONTEXT } from '../context/has-children.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEntityHasChildrenCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_HAS_CHILDREN_ENTITY_CONTEXT, (context) => {
			this.observe(context.hasChildren, (hasChildren) => {
				this.permitted = hasChildren === true;
			});
		});
	}
}

export { UmbEntityHasChildrenCondition as api };
