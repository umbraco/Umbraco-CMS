import { UMB_MENU_CONTEXT } from '../components/menu/menu.context.js';
import { UmbConditionBase } from '../../extension-registry/conditions/condition-base.controller.js';
import type { MenuAliasConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

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
