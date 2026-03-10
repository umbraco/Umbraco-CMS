import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '../property-context/property-sort-mode.context-token.js';
import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';

/**
 * Property action that toggles the property sort mode.
 * When executed, it retrieves the sort mode context and toggles its state.
 */
export class UmbPropertySortModePropertyAction extends UmbPropertyActionBase {
	/**
	 * Executes the property action by toggling the sort mode state.
	 * @returns {Promise<void>} A promise that resolves when the action is complete.
	 */
	override async execute(): Promise<void> {
		const context = await this.getContext(UMB_PROPERTY_SORT_MODE_CONTEXT);
		context?.toggleIsSortMode();
	}
}

export { UmbPropertySortModePropertyAction as api };
