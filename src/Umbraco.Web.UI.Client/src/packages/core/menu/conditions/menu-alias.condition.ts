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

		console.error(
			'Condition of alias `Umb.Condition.MenuAlias` is not implemented. Please report this issue if you where expecting this condition to work.',
		);
		/*
		this.consumeContext(UMB_MENU_CONTEXT, (context) => {
			this.observe(
				context.alias,
				(MenuAlias) => {
					this.permitted = MenuAlias === this.config.match;
				},
				'observeAlias',
			);
		});
		*/
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Menu Alias Condition',
	alias: 'Umb.Condition.MenuAlias',
	api: UmbMenuAliasCondition,
};
