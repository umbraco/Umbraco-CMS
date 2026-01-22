import { UMB_PROPERTY_SORT_MODE_CONTEXT } from './property-sort-mode.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Context for managing the property sort mode state.
 * Provides reactive state management for enabling/disabling sort mode on properties.
 */
export class UmbPropertySortModeContext extends UmbContextBase {
	#isSortMode = new UmbBooleanState(false);

	/**
	 * Observable that emits the current sort mode state.
	 */
	readonly isSortMode = this.#isSortMode.asObservable();

	/**
	 * Creates an instance of UmbPropertySortModeContext.
	 * @param {UmbControllerHost} host - The controller host that this context belongs to.
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_SORT_MODE_CONTEXT);
	}

	/**
	 * Gets the current sort mode state.
	 * @returns {boolean | undefined} The current sort mode state, or undefined if not set.
	 */
	getIsSortMode(): boolean | undefined {
		return this.#isSortMode.getValue();
	}

	/**
	 * Sets the sort mode state.
	 * @param {boolean} isSortMode - Whether sort mode should be enabled.
	 */
	setIsSortMode(isSortMode: boolean) {
		this.#isSortMode.setValue(isSortMode);
	}

	/**
	 * Toggles the sort mode state between enabled and disabled.
	 */
	toggleIsSortMode() {
		const enabled = this.getIsSortMode();
		this.setIsSortMode(!enabled);
	}
}

export { UmbPropertySortModeContext as api };
