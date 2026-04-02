import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MENU_ITEM_CONTEXT } from '@umbraco-cms/backoffice/menu';

export class ExampleCollapseMenuItemEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		const context = await this.getContext(UMB_MENU_ITEM_CONTEXT);
		context?.expansion.collapseAll();
	}
}

export { ExampleCollapseMenuItemEntityAction as api };
