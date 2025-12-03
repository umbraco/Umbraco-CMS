import { UMB_PROPERTY_SORT_MODE_CONTEXT } from './property-sort-mode.context-token.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPropertySortModeContext extends UmbContextBase {
	#isSortMode = new UmbBooleanState(false);
	readonly isSortMode = this.#isSortMode.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_SORT_MODE_CONTEXT);
	}

	getIsSortMode(): boolean | undefined {
		return this.#isSortMode.getValue();
	}

	setIsSortMode(sortingMode: boolean) {
		this.#isSortMode.setValue(sortingMode);
	}

	toggleIsSortMode() {
		const enabled = this.getIsSortMode();
		this.setIsSortMode(!enabled);
	}
}

export { UmbPropertySortModeContext as api };
