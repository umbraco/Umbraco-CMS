import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { UmbWorkspaceActionMenuItemBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/workspace';

export class ExampleResetCounterMenuItemAction
	extends UmbWorkspaceActionMenuItemBase
	implements UmbWorkspaceActionMenuItem
{
	/**
	 * This method is executed when the menu item is clicked
	 */
	override async execute() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		if (!context) {
			throw new Error('Could not get the counter context');
		}

		// Reset the counter to 0
		context.reset();
	}
}

export const api = ExampleResetCounterMenuItemAction;
