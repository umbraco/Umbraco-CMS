import { UMB_MENU_CONTEXT } from '../../menu/components/menu/menu.context.js';
import { UmbConditionBase } from './condition-base.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export type MenuAliasConditionConfig = UmbConditionConfigBase & {
	match: string;
};

export class UmbMenuAliasCondition extends UmbConditionBase<MenuAliasConditionConfig> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<MenuAliasConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_MENU_CONTEXT, (context) => {
			this.observe(
				context.alias,
				(MenuAlias) => {
					this.permitted = MenuAlias === this.config.match;
				},
				'observeAlias',
			);
		});
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Menu Alias Condition',
	alias: 'Umb.Condition.MenuAlias',
	api: UmbMenuAliasCondition,
};
