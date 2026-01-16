import type { UmbContentTreeItemTypeUniqueConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_TREE_ITEM_CONTEXT } from '@umbraco-cms/backoffice/tree';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbContentTreeItemTypeUniqueCondition
	extends UmbConditionBase<UmbContentTreeItemTypeUniqueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbContentTreeItemTypeUniqueConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (context) => {
			this.observe((context as any)?.typeUnique, this.#check, '_UmbContentTreeItemTypeUniqueCondition');
		});
	}

	#check = (value: string | undefined) => {
		if (value === undefined) {
			this.permitted = false;
			return;
		}

		// if the config has a match, we only check that
		if (this.config.match !== undefined) {
			this.permitted = value === this.config.match;
			return;
		}

		this.permitted = this.config.oneOf?.some((configValue) => configValue === value) ?? false;
	};
}

export { UmbContentTreeItemTypeUniqueCondition as api };
