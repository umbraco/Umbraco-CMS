import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_TREE_ITEM_CONTEXT } from '@umbraco-cms/backoffice/tree';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateAllowDeleteActionCondition extends UmbConditionBase<never> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<never>) {
		super(host, args);

		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (context) => {
			this.observe(
				context.hasChildren,
				(hasChildren) => {
					this.permitted = hasChildren === false;
				},
				'_templateAllowDeleteActionCondition',
			);
		});
	}
}

export { UmbTemplateAllowDeleteActionCondition as api };
